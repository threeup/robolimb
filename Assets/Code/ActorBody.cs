using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BasicCommon;



public class ActorBody : MonoBehaviour 
{
	public Actor thisActor;
	public Animator thisAnimator;
	public List<ActorBodyPart> bodyParts;
	public Color mainColor = Color.white;

	float defaultRegrow = 0.6f;
	public BasicTimer superTimer = new BasicTimer(0f);
	public BasicTimer regrowTimer = new BasicTimer(0.1f);

	BodyPartType weaponType = BodyPartType.ArmLeftLower;
	ActorBodyPart weapon = null;
	ActorBodyPart projectile = null;
	ActorBodyPart thrower = null;

	float maxForce = 1800f;
	public Vector3 currentDirection = Vector3.zero;

	public GameObject skeleton;

	public bool canWalkNormal = true;
	public bool canWalkHop = false;
	public bool canWalkKneel = false;
	public bool hasHead = true;

	List<ActorBodyPart> regrowList = new List<ActorBodyPart>();

	// Use this for initialization
	void Awake () 
	{
		weapon = null;
		thrower = null;
	}

	public void Spawn(Color mainColor)
	{
		this.mainColor = mainColor;
		foreach(ActorBodyPart bp in bodyParts)
		{
			bp.SetColor(mainColor);
		}
	}
	
	public bool IsSuper()
	{
		return !superTimer.IsPaused;
	}

	// Update is called once per frame
	void Update () 
	{
		float deltaTime = Time.deltaTime;
		superTimer.Tick(deltaTime);
		if( hasHead && regrowList.Count > 0 && regrowTimer.Tick(deltaTime) )
		{
			Regrow();
			regrowTimer.Duration = defaultRegrow/(IsSuper() ? 4f : 1f);
			regrowTimer.Reset();
		}
		currentDirection = thisActor.ThrowVector();


		if(Input.GetKey(KeyCode.P))
		{
			Test();
		}
	}

	public bool CanThrowStart()
	{
		GetWeapon();
		bool result = thrower != null && thrower.CanThrowStart() && weapon != null && weapon.CanWeaponStart();
		return result;
	}

	public bool CanThrowFinish()
	{
		return thrower != null && thrower.CanThrowFinish() && weapon != null && weapon.CanWeaponFinish();
	}

	void Regrow()
	{

		ActorBodyPart bp = regrowList[0];
		while( bp.parentPart != null && regrowList.Contains(bp.parentPart) )
		{
			bp = bp.parentPart;
		}
		regrowList.Remove(bp);
		bp.growSpeed = IsSuper() ? 4f : 1f;
		bp.machine.SetState(PartState.GROWING);
	}

	
	public void AlignToThrower()
	{
		weapon.handParent = thrower.transform;
		
		thrower.machine.SetState(PartState.GRABBING);
		weapon.machine.SetState(PartState.ALIGNING);
		EvaluateSelf();
	}

	public void Animate()
	{
		thrower.machine.SetState(PartState.ANIMATING);
	}

	public void Launch(float amount)
	{
		projectile = weapon;
		StartCoroutine(LaunchRoutine(amount));

	}
	IEnumerator LaunchRoutine(float amount)
	{
		while(projectile.stateLock)
		{
			yield return new WaitForSeconds(0.1f);
		}
		amount = 0.33f+0.67f*amount;
		thrower.machine.SetState(PartState.ATTACHED);
		SelectWeapon(null);
		yield return null;
		projectile.Launch(amount*maxForce, currentDirection);
		Deregister(projectile);
		thrower = null;
		yield return null;
		EvaluateSelf();
	}

	public void SelectWeapon(ActorBodyPart bp)
	{
		if( weapon != null )
		{
			if( !weapon.CanWeaponDeselect() )
			{
				Debug.Log("Failed to deselect"+weapon+" "+weapon.debugState);
				return;
			}
			weapon.Select(false);
			weapon = null;
		}
		if( bp == null )
		{
			return;
		}
		if( bp.CanWeaponSelect() )
		{
			weapon = bp;
			GetThrower();
			if( thrower == null )
			{
				weapon = null;
				return;
			}
			weapon.Select(true);
		}
	}
	public void GetWeapon()
	{
		SelectWeapon( bodyParts.Find(x=>x.bodyPartType == weaponType && x.CanWeaponSelect()) );
		if( weapon == null )
		{
			Cycle();
		}
	}
	public void Cycle()
	{
		for(int i=0; i<10; ++i)
		{
			switch(weaponType)
			{
				case BodyPartType.ArmRightUpper: weaponType = BodyPartType.ArmRightLower; break;
				case BodyPartType.ArmRightLower: weaponType = BodyPartType.ArmLeftUpper; break;
				default:
				case BodyPartType.ArmLeftUpper: weaponType = BodyPartType.ArmLeftLower; break;
				case BodyPartType.ArmLeftLower: weaponType = BodyPartType.LegRightUpper; break;
				case BodyPartType.LegRightUpper: weaponType = BodyPartType.LegRightLower; break;
				case BodyPartType.LegRightLower: weaponType = BodyPartType.LegLeftUpper; break;
				case BodyPartType.LegLeftUpper: weaponType = BodyPartType.LegLeftLower; break;
				case BodyPartType.LegLeftLower: weaponType = BodyPartType.ArmRightUpper; break;
			}
			ActorBodyPart nextWeapon = bodyParts.Find(x=>x.bodyPartType == weaponType && x.CanWeaponSelect());
			SelectWeapon(nextWeapon);
			if( weapon != null )
			{
				break;
			}
		}
		
	}

	public void GetThrower()
	{
		thrower = null;
		if( weaponType != BodyPartType.None )
		{
			bool weaponRight = IsRightArm(weaponType);
			bool weaponLeft = IsLeftArm(weaponType);
			if(weaponRight)
			{
				thrower = bodyParts.Find(x=>x.bodyPartType == BodyPartType.ArmLeftHand && x.CanThrowStart());
			}
			else if(weaponLeft)
			{
				thrower = bodyParts.Find(x=>x.bodyPartType == BodyPartType.ArmRightHand && x.CanThrowStart());
			}
			else
			{
				if( thrower == null )
				{
					thrower = bodyParts.Find(x=>x.bodyPartType == BodyPartType.ArmRightHand && x.CanThrowStart());
				}
				if( thrower == null )
				{
					thrower = bodyParts.Find(x=>x.bodyPartType == BodyPartType.ArmLeftHand && x.CanThrowStart());
				}
			}
		}
	}
	
	bool IsLeftArm(BodyPartType part)
	{
		switch(part)
		{
			case BodyPartType.ArmLeftHand:
			case BodyPartType.ArmLeftLower:
			case BodyPartType.ArmLeftElbow:
			case BodyPartType.ArmLeftUpper:
				return true;
			default:
				return false;
		}
	}
	bool IsRightArm(BodyPartType part)
	{
		switch(part)
		{
			case BodyPartType.ArmRightHand:
			case BodyPartType.ArmRightLower:
			case BodyPartType.ArmRightElbow:
			case BodyPartType.ArmRightUpper:
				return true;
			default:
				return false;
		}
	}


	public void Register(ActorBodyPart bp)
	{
		
		bodyParts.Add(bp);
		EvaluateSelf();
	}
	void Deregister(ActorBodyPart bp)
	{
		bodyParts.Remove(bp);
		if( bp.childPart != null )
		{
			Deregister(bp.childPart);
		}
	}

	public void Dormant(ActorBodyPart bp)
	{
		if( bp.bodyPartType == BodyPartType.Head)
		{
			Debug.Log("Death!");
			foreach(ActorBodyPart bodyPart in bodyParts)
			{
				bodyPart.Expire();
			}
		}
		else
		{
			regrowList.Add(bp);
		}
	}

	public void EvaluateSelf()
	{
		ActorBodyPart legLowerLeft = bodyParts.Find(x=>x.bodyPartType == BodyPartType.LegLeftLower && x.CanWalk()); 
		ActorBodyPart legLowerRight = bodyParts.Find(x=>x.bodyPartType == BodyPartType.LegRightLower && x.CanWalk()); 
		ActorBodyPart legUpperLeft = bodyParts.Find(x=>x.bodyPartType == BodyPartType.LegLeftUpper && x.CanWalk()); 
		ActorBodyPart legUpperRight = bodyParts.Find(x=>x.bodyPartType == BodyPartType.LegRightUpper && x.CanWalk()); 
		ActorBodyPart head = bodyParts.Find(x=>x.bodyPartType == BodyPartType.Head && x.CanWalk()); 
		
		hasHead = head != null;
		canWalkNormal = legLowerLeft != null && legLowerRight != null;
		canWalkHop = legLowerLeft != null || legLowerRight != null;
		canWalkKneel = legUpperLeft != null && legUpperRight != null && legLowerLeft == null && legLowerRight == null;
		if( canWalkNormal )
		{
			skeleton.transform.position = Vector3.zero;

		}
		else if( canWalkKneel )
		{
			skeleton.transform.position = 0.31f*Vector3.down;

		}
		else
		{	

		}
	}

	public float GetSpeed()
	{

        if( canWalkNormal )
        {
        	return 1f;
        }
        else if( canWalkHop )
        {
            return 0.3f;       
        }
        else if( canWalkKneel )
        {
            return 0.2f;       
        }
        else
        {
            return 0.05f;
        }
	}

	public void Test()
	{
		ActorBodyPart legLowerLeft = bodyParts.Find(x=>x.bodyPartType == BodyPartType.LegLeftLower);
		legLowerLeft.Glue(GlueState.SKELETON);
	}
}
