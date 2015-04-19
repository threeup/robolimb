using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BasicCommon;



public class ActorBody : MonoBehaviour 
{

	public Animator thisAnimator;
	public List<ActorBodyPart> bodyParts;

	public BasicTimer regrowTimer = new BasicTimer(2f);

	BodyPartType weaponType = BodyPartType.ArmLeftLower;
	ActorBodyPart weapon = null;
	BodyPartType throwerType = BodyPartType.ArmRightHand;
	ActorBodyPart thrower = null;

	float maxForce = 1200f;
	public Vector3 currentDirection = Vector3.zero;

	public GameObject skeleton;

	public bool canWalkNormal = true;
	public bool canWalkKneel = true;

	List<ActorBodyPart> regrowList = new List<ActorBodyPart>();

	// Use this for initialization
	void Awake () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		float deltaTime = Time.deltaTime;
		if( regrowList.Count > 0 && regrowTimer.Tick(deltaTime) )
		{
			Regrow();
		}
		currentDirection = this.transform.forward;
	}

	public bool CanThrowStart()
	{
		GetThrower();
		GetWeapon();
		return thrower != null && thrower.CanThrowStart() && weapon != null && weapon.CanWeaponStart();
	}

	public bool CanThrowFinish()
	{
		GetThrower();
		GetWeapon();
		return thrower != null && thrower.CanThrowFinish() && weapon != null && weapon.CanWeaponFinish();
	}

	void Regrow()
	{
		regrowList.Sort(delegate(ActorBodyPart p1, ActorBodyPart p2)
			{
			    return p1.depth.CompareTo(p2.depth);
			});

		ActorBodyPart bp = regrowList[0];
		regrowList.RemoveAt(0);
		bp.machine.SetState(PartState.GROWING);
	}

	
	public void AlignToThrower()
	{
		GetThrower();
		GetWeapon();
		weapon.handParent = thrower.transform;
		
		thrower.machine.SetState(PartState.GRABBING);
		weapon.machine.SetState(PartState.ALIGNING);
	}

	public void Animate()
	{
		thrower.machine.SetState(PartState.ANIMATING);
	}

	public void Launch(float amount)
	{
		thrower.machine.SetState(PartState.ATTACHED);
		weapon.Launch(amount*maxForce, currentDirection);
		Deregister(weapon);
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
				return;
			}
		}
		weapon = null;
		


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
			weapon = bodyParts.Find(x=>x.bodyPartType == weaponType);
			if( weapon != null && SameArm(weaponType,throwerType) )
			{
				BodyPartType switchThrowerType = throwerType == BodyPartType.ArmRightHand ? BodyPartType.ArmLeftHand : BodyPartType.ArmRightHand;
				ActorBodyPart switchThrower = bodyParts.Find(x=>x.bodyPartType == switchThrowerType);
				if( switchThrower == null )
				{
					weapon = null;
				}
				else
				{
					thrower = switchThrower;
					throwerType = switchThrowerType;
				}				
			}
		}
		if( weapon == null )
		{
			Debug.Log("Failed to cycle"+weaponType);
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
		if( throwerType != BodyPartType.None )
		{
			thrower = bodyParts.Find(x=>x.bodyPartType == throwerType);
		}
	}
	

	bool SameArm(BodyPartType first, BodyPartType second)
	{
		return (IsLeftArm(first) && IsLeftArm(second)) || (IsRightArm(first) && IsRightArm(second));
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
		regrowList.Add(bp);
	}

	public void EvaluateSelf()
	{
		ActorBodyPart legLowerLeft = bodyParts.Find(x=>x.bodyPartType == BodyPartType.LegLeftLower); 
		ActorBodyPart legLowerRight = bodyParts.Find(x=>x.bodyPartType == BodyPartType.LegRightLower); 
		ActorBodyPart legUpperLeft = bodyParts.Find(x=>x.bodyPartType == BodyPartType.LegLeftUpper); 
		ActorBodyPart legUpperRight = bodyParts.Find(x=>x.bodyPartType == BodyPartType.LegRightUpper); 
		ActorBodyPart handLeft = bodyParts.Find(x=>x.bodyPartType == BodyPartType.ArmLeftHand); 
		ActorBodyPart handRight = bodyParts.Find(x=>x.bodyPartType == BodyPartType.ArmRightHand); 
		
		canWalkNormal = legLowerLeft != null && legLowerRight != null;
		canWalkKneel = legUpperLeft != null && legUpperRight != null && legLowerLeft == null && legLowerRight == null;
		if( handRight != null )
		{
			throwerType = BodyPartType.ArmRightHand;	
		}
		else if( handLeft != null )
		{
			throwerType = BodyPartType.ArmLeftHand;
		}
		else
		{
			throwerType = BodyPartType.None;
		}	
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
