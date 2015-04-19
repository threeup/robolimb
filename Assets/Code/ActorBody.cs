using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BasicCommon;



public class ActorBody : MonoBehaviour 
{

	public Animator thisAnimator;
	public List<ActorBodyPart> bodyParts;
	public Color mainColor;

	public BasicTimer regrowTimer = new BasicTimer(2f);

	BodyPartType weaponType = BodyPartType.ArmLeftLower;
	ActorBodyPart weapon = null;
	BodyPartType throwerType = BodyPartType.ArmRightHand;
	ActorBodyPart thrower = null;

	float maxForce = 1200f;
	public Vector3 currentDirection = Vector3.zero;

	public GameObject skeleton;

	public bool canWalkNormal = true;
	public bool canWalkHop = false;
	public bool canWalkKneel = false;

	List<ActorBodyPart> regrowList = new List<ActorBodyPart>();

	// Use this for initialization
	void Awake () 
	{
		
	}

	public void Spawn(Color mainColor)
	{
		this.mainColor = mainColor;
		foreach(ActorBodyPart bp in bodyParts)
		{
			bp.SetColor(mainColor);
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		float deltaTime = Time.deltaTime;
		if( regrowList.Count > 0 && regrowTimer.Tick(deltaTime) )
		{
			Regrow();
		}
		currentDirection = 0.7f*this.transform.forward+0.5f*this.transform.up;
		currentDirection.Normalize();
	}

	public bool CanThrowStart()
	{
		GetWeapon();
		GetThrower();
		bool result = thrower != null && thrower.CanThrowStart() && weapon != null && weapon.CanWeaponStart();
		if( !result )
		{
			Debug.Log("Cant Throw"+thrower+" "+weapon);
		}
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
		bp.machine.SetState(PartState.GROWING);
	}

	
	public void AlignToThrower()
	{
		GetWeapon();
		GetThrower();
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
		amount = 0.33f+0.67f*amount;
		thrower.machine.SetState(PartState.ATTACHED);
		weapon.Launch(amount*maxForce, currentDirection);
		Deregister(weapon);
		weapon = null;
		thrower = null;
		EvaluateSelf();
	}

	public void Cycle()
	{
		if( weapon != null )
		{
			if( weapon.CanWeaponDeselect() )
			{
				weapon.Select(false);
			}
			else
			{
				Debug.Log(weapon+" cant cycle "+weapon.debugState);
				return;
			}
			weapon = null;
		}
		


		for(int i=0; i<10 && weapon == null; ++i)
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
			weapon = bodyParts.Find(x=>x.bodyPartType == weaponType && x.CanWeaponSelect());
			if( weapon != null )
			{
				GetThrower();
				if( thrower == null )
				{
					weapon = null;
				}
			}
		}
		if( weapon == null )
		{
			Debug.Log("Failed to cycle"+weaponType+" "+throwerType);
		}
		if( weapon != null )
		{
			weapon.Select(true);
		}
		//Debug.Log("select"+weaponType+" "+throwerType+" "+thrower);
	}

	public void GetThrower()
	{
		thrower = null;
		throwerType = BodyPartType.None;
		if( weaponType != BodyPartType.None )
		{
			bool weaponRight = IsRightArm(weaponType);
			bool weaponLeft = IsLeftArm(weaponType);
			if(weaponRight)
			{
				throwerType = BodyPartType.ArmLeftHand;
				thrower = bodyParts.Find(x=>x.bodyPartType == throwerType && x.CanThrowStart());
			}
			else if(weaponLeft)
			{
				throwerType = BodyPartType.ArmRightHand;	
				thrower = bodyParts.Find(x=>x.bodyPartType == throwerType && x.CanThrowStart());
			}
			else
			{
				if( thrower == null )
				{
					throwerType = BodyPartType.ArmRightHand;
					thrower = bodyParts.Find(x=>x.bodyPartType == throwerType && x.CanThrowStart());
				}
				if( thrower == null )
				{
					throwerType = BodyPartType.ArmLeftHand;
					thrower = bodyParts.Find(x=>x.bodyPartType == throwerType && x.CanThrowStart());
				}
			}
		}
		if( thrower == null )
		{
			throwerType = BodyPartType.None;
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
	public void GetWeapon()
	{
		weapon = bodyParts.Find(x=>x.bodyPartType == weaponType);
		if( weapon == null )
		{
			Cycle();
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
}
