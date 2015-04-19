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

	public bool canThrow = true;
	public bool canWalkNormal = true;
	public bool canWalkKneel = true;

	List<ActorBodyPart> regrowList = new List<ActorBodyPart>();

	// Use this for initialization
	void Awake () 
	{
		ActorBodyPart[] bpList = GetComponentsInChildren<ActorBodyPart>();
		bodyParts = new List<ActorBodyPart>(bpList);	
		foreach(ActorBodyPart bp in bodyParts)
		{
			bp.thisBody = this;
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
		currentDirection = this.transform.forward;
	}

	void Regrow()
	{
		regrowList.Sort(delegate(ActorBodyPart p1, ActorBodyPart p2)
			{
			    return p1.depth.CompareTo(p2.depth);
			});
		
		ActorBodyPart bp = regrowList[0];
		regrowList.RemoveAt(0);
		bp.Grow();
	}

	
	public void AlignToThrower()
	{
		GetThrower();
		GetWeapon();
		ActorBodyPart bp = weapon;
		bp.AlignTo(thrower);
	}

	public void Animate()
	{
		ActorBodyPart bp = thrower;
		bp.Animate();
	}

	public void Launch(float amount)
	{
		ActorBodyPart bp = weapon;
		bp.Launch(amount*maxForce, currentDirection);
		Deregister(bp);
		EvaluateSelf();
	}

	public void Cycle()
	{
		if( weapon != null )
		{
			weapon.Select(false);
		}
		weapon = null;
		for(int i=0; i<10 && weapon == null; ++i)
		{
			int weaponTypeIdx = (int)weaponType + 1;
			if( weaponTypeIdx < 10 || weaponTypeIdx > 17 )
			{
				weaponTypeIdx = 10;
			}
			weaponType = (BodyPartType)weaponTypeIdx;
			weapon = bodyParts.Find(x=>x.bodyPartType == weaponType);
			if( weapon != null && SameArm(weaponType,throwerType) )
			{
				BodyPartType switchThrowerType = throwerType == BodyPartType.ArmRightHand ? BodyPartType.ArmLeftHand : BodyPartType.ArmRightHand;
				ActorBodyPart switchThrower = bodyParts.Find(x=>x.bodyPartType == switchThrowerType);
				if( switchThrower == null )
				{
					weapon = null;
					Debug.Log("cant use"+weaponType+" with "+throwerType);
				}
				else
				{
					thrower = switchThrower;
					throwerType = switchThrowerType;
				}				
			}
			if( weapon == null )
			{
				Debug.Log("missing"+weaponType);
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
	}

	public void GetThrower()
	{
		thrower = bodyParts.Find(x=>x.bodyPartType == throwerType);
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

	public void Reregister(ActorBodyPart bp)
	{
		bodyParts.Add(bp);
		EvaluateSelf();
	}
	void Deregister(ActorBodyPart bp)
	{
		bodyParts.Remove(bp);
		regrowList.Add(bp);
		
		if( bp.childPart != null )
		{
			Deregister(bp.childPart);
		}
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
		canThrow = throwerType != BodyPartType.None;
		if( canWalkNormal )
		{
			thisAnimator.enabled = true;
			skeleton.transform.position = Vector3.zero;
		}
		else if( canWalkKneel )
		{
			thisAnimator.enabled = true;
			skeleton.transform.position = 0.31f*Vector3.down;
		}
		else
		{
			thisAnimator.enabled = false;
		}
	}
}
