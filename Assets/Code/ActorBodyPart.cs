using UnityEngine;
using System.Collections;
using DG.Tweening;
using BasicCommon;
using GumboLib;

public enum BodyPartType
{
	Head = 0,
	Torso = 1,
	ArmLeftUpper = 10,
	ArmRightUpper = 11,
	ArmLeftElbow = 12,
	ArmRightElbow = 13,
	ArmLeftLower = 14,
	ArmRightLower = 15,
	ArmLeftHand = 16,
	ArmRightHand = 17,
	

	LegLeftUpper = 20,
	LegRightUpper = 21,
	LegLeftKnee = 22,
	LegRightKnee = 23,
	LegLeftLower = 24,
	LegRightLower = 25,
	LegLeftFoot = 26,
	LegRightFoot = 27,
	None = 99,
}

public enum PartState
{
	NONE,
	ATTACHED,
	SELECTED,
	GRABBING,
	ALIGNING,
	ANIMATING,
	HELD,
	GLUED,
	LAUNCH,
	DORMANT,
	GROWING,
}

public enum PhysicsState
{
	NORMAL,
	FLYINGOFF,
	FLYINGON,
}

// upper arm
// selected, aligning, held, launch, dormant
// lower arm
// attached, glued, dormant

public enum GlueState
{
	HOME,
	SKELETON,
	HAND,
	LIMB,
	FREE,
}

public class ActorBodyPart : MonoBehaviour 
{


	public ActorBody thisBody;
	public Transform thisTransform;
	public Renderer thisRenderer;

	public Rigidbody thisRigidbody;
	public Collider thisCollider;
	public BodyPartType bodyPartType;
	public ActorBodyPart parentPart;
	public ActorBodyPart childPart;

	private Transform bodyParent = null;
	public Transform handParent = null;
	private Transform limbParent = null;
	private Vector3 originalPos;
	private Quaternion originalRot;
	private Vector3 originalScale;

	//private BasicTimer colliderTimer = new BasicTimer(0f);
	private BasicTimer expireTimer = new BasicTimer(0f);
	private bool waitingForProjectile = false;

	public BasicMachine<PartState> machine;

	public int depth = -1;

	public bool stateLock = false;
	private GlueState glueState = GlueState.HOME;
	private PhysicsState physicsState = PhysicsState.NORMAL;
	public PartState debugState;

	public float growSpeed = 1f;
	float defaultGrowTime = 5f;

	private Vector3 nextForce = Vector3.zero;

	Color selectedColor = new Color(0.5f, 1f, 0.8f, 1f);

	void Awake()
	{
		machine = new BasicMachine<PartState>();
		machine.Initialize(typeof(PartState));
		machine.OnChange = OnChange;
		machine[(int)PartState.ATTACHED].OnEnter = OnAttach;
		machine[(int)PartState.ATTACHED].CanEnter = CanSwitch;
		machine[(int)PartState.ALIGNING].OnEnter = OnAlign;
		machine[(int)PartState.ALIGNING].CanEnter = CanSwitch;
		machine[(int)PartState.ANIMATING].OnEnter = OnAnimate;
		machine[(int)PartState.ANIMATING].CanEnter = CanSwitch;
		machine[(int)PartState.HELD].OnEnter = OnHeld;
		machine[(int)PartState.HELD].CanEnter = CanSwitch;
		machine[(int)PartState.GLUED].OnEnter = OnGlued;
		machine[(int)PartState.GLUED].CanEnter = CanSwitch;
		machine[(int)PartState.LAUNCH].OnEnter = OnLaunch;
		machine[(int)PartState.LAUNCH].CanEnter = CanSwitch;
		machine[(int)PartState.DORMANT].OnEnter = OnDormant;
		machine[(int)PartState.DORMANT].CanEnter = CanSwitch;
		machine[(int)PartState.GROWING].OnEnter = OnGrow;
		machine[(int)PartState.GROWING].CanEnter = CanSwitch;

		thisTransform = this.transform;
		thisRenderer = this.GetComponent<Renderer>();
		thisBody = this.GetComponentInParent<ActorBody>();
		bodyParent = thisTransform.parent;
		handParent = null;
		limbParent = parentPart != null ? parentPart.transform : null;
		originalPos = thisTransform.localPosition;
		originalRot = thisTransform.localRotation;
		originalScale = thisTransform.localScale;
		if( thisBody != null )
		{
			machine.SetState(PartState.ATTACHED);
		}
		CheckDepth();
	}

	public bool CanThrowStart()
	{
		return machine.IsState(PartState.ATTACHED);
	}
	public bool CanThrowFinish()
	{
		return machine.IsState(PartState.ANIMATING);
	}

	public bool CanWeaponStart()
	{
		return machine.IsState(PartState.SELECTED);
	}
	public bool CanWeaponFinish()
	{
		return machine.IsState(PartState.HELD);
	}
	public bool CanWeaponSelect()
	{
		return machine.IsState(PartState.SELECTED) || machine.IsState(PartState.ATTACHED);
	}
	public bool CanWeaponDeselect()
	{
		return machine.IsState(PartState.SELECTED) || machine.IsState(PartState.HELD) || 
				machine.IsState(PartState.ATTACHED) || machine.IsState(PartState.LAUNCH);
	}

	public bool CanWalk()
	{
		return machine.IsState(PartState.SELECTED) || machine.IsState(PartState.ATTACHED);
	}

	bool CanSwitch()
	{
		return !stateLock;
	}

	public int CheckDepth()
	{
		if( depth < 0 )
		{
			depth = 1;
			if( parentPart != null )
			{
				depth += parentPart.CheckDepth();
			}
		}
		return depth;
	}

	public void Update()
	{
		float deltaTime = Time.deltaTime;
		if( waitingForProjectile )
		{
			
			Vector3 diff = this.transform.position - thisBody.skeleton.transform.position;
			if( Mathf.Abs(diff.x) + Mathf.Abs(diff.z) > 0.5f )
			{
				if( physicsState == PhysicsState.FLYINGOFF )
				{
					PhysicsChange(PhysicsState.FLYINGON);
				}
				waitingForProjectile = false;
			}
			
		}
		
		if( expireTimer.Tick(deltaTime) )
		{
			Expire();
		}
		
	}

	public void Expire()
	{
		expireTimer.Pause(true);
		GlueRecursive(GlueState.FREE);
		SetStateRecursive(PartState.DORMANT);
	}

	void StateLock(bool val)
	{
		stateLock = val;
		if( !val )
		{
			if( machine.failedState != null)
			{
				machine.RetryFailedState();
			}
		}
	}

	public void SetStateChildren(PartState nextState)
	{
		if( childPart != null )
		{
			childPart.SetStateRecursive(nextState);
		}
	}
	public void SetStateRecursive(PartState nextState)
	{
		machine.SetState(nextState);
		if( childPart != null )
		{
			childPart.SetStateRecursive(nextState);
		}
	}
	public void SetLayerRecursive(LayerMask layer, bool colliderIsTrigger)
	{
		this.gameObject.layer = layer;
		thisCollider.isTrigger = colliderIsTrigger; 
		SetColor(thisBody.mainColor);
		if( childPart != null )
		{
			childPart.SetLayerRecursive(layer, colliderIsTrigger);
		}
	}

	void OnAttach()
	{
		thisBody.Register(this);

		Glue(GlueState.HOME);
		thisTransform.localPosition = originalPos;
		thisTransform.localRotation = originalRot;
		
	}

	void OnAlign()
	{
		StartCoroutine(AlignRoutine());
	}

	IEnumerator AlignRoutine()
	{
		Glue(GlueState.SKELETON);
		yield return null;
		SetStateChildren(PartState.GLUED);
		yield return null;
		StateLock(true);
		thisTransform.DOMove(handParent.position, 1).OnComplete(SetStateHeld);
	}

	void SetStateHeld()
	{
		StateLock(false);
		machine.SetState(PartState.HELD);
	}

	void OnDormant()
	{
		GameObject debris = Instantiate(this.gameObject, thisTransform.position, thisTransform.rotation) as GameObject;
		debris.layer = LayerMask.NameToLayer("Projectile");
		debris.transform.localScale = originalScale;
		ActorBodyPart debrisPart = debris.GetComponent<ActorBodyPart>();
		//debrisPart.thisRenderer.material = new Material(thisRenderer.material);
		debrisPart.thisRenderer.material.color = Color.green;
		debrisPart.thisRigidbody.velocity = thisRigidbody.velocity;
		//debrisPart.SetParent(thisTransform.parent);
		Destroy(debrisPart);

		thisTransform.localScale = 0.0001f*Vector3.one;
		if( thisBody != null )
		{
			thisBody.Dormant(this);
		}
		else
		{
			Debug.Log("Missing body"+this.gameObject);
			Destroy (this.gameObject);
		}
		
	}


	void SetParent(Transform nextParent)
	{
		thisTransform.SetParent(nextParent, true);
	}

	void OnGrow()
	{
		Glue(GlueState.HOME);
		thisTransform.localPosition = originalPos;
		thisTransform.localRotation = originalRot;
		StateLock(true);
		thisTransform.DOScale(originalScale, defaultGrowTime/growSpeed).OnComplete(Reregister);	
	}

	public void PhysicsChange(PhysicsState nextPhysicsState)
	{
		if( physicsState == nextPhysicsState )
		{
			return;
		}
		physicsState = nextPhysicsState;	
		switch(physicsState)
		{
			case PhysicsState.NORMAL: 
				this.gameObject.layer = LayerMask.NameToLayer("Limb");
				thisCollider.isTrigger = false;
				break;
			case PhysicsState.FLYINGON:
				this.gameObject.layer = LayerMask.NameToLayer("Projectile");
				thisCollider.isTrigger = false;
				break;
			case PhysicsState.FLYINGOFF:
				this.gameObject.layer = LayerMask.NameToLayer("Projectile");
				thisCollider.isTrigger = true;
				break;
		}
	}

	public void Glue(GlueState nextGlueState)
	{
		if( glueState == nextGlueState )
		{
			return;
		}
		glueState = nextGlueState;

		switch(glueState)
		{
			case GlueState.HOME: SetParent(bodyParent); break;
			case GlueState.SKELETON: 
				SetParent(thisBody.skeleton.transform); 
				break;
			case GlueState.HAND: SetParent(handParent); break;
			case GlueState.LIMB: SetParent(limbParent); break;
			case GlueState.FREE: 
				SetParent(null); 
				break;
		}
		if(glueState == GlueState.FREE)
		{
			thisRigidbody.isKinematic = false;
			PhysicsChange(PhysicsState.FLYINGOFF);
			waitingForProjectile = true;
		}
		else
		{
			thisRigidbody.isKinematic = true;
			PhysicsChange(PhysicsState.NORMAL);
		}
	}

	public void GlueRecursive(GlueState nextGlueState)
	{
		Glue(nextGlueState);
		if(childPart != null)
		{
			childPart.GlueRecursive(nextGlueState);
		}
	}

	void OnHeld()
	{
		Glue(GlueState.HAND);
		StateLock(true);
		thisTransform.DOLocalMove(Vector3.zero, 0.2f).OnComplete(Unlock);
	}

	void OnGlued()
	{
		Glue(GlueState.LIMB);
	}

	void Unlock()
	{
		StateLock(false);
	}

	void OnAnimate()
	{	
		
	}

	public void Launch(float force, Vector3 direction)
	{
		nextForce = force*direction;
		machine.SetState(PartState.LAUNCH);
	}	

	void OnLaunch()
	{
		thisBody.Cycle();
		Glue(GlueState.FREE);

		//test
		//thisTransform.position = thisTransform.position + Vector3.one*10f;
		thisRigidbody.AddForce(nextForce);
		
		expireTimer = new BasicTimer(1.5f, false);
	}

	public void Reregister()
	{
		StateLock(false);
		machine.SetState(PartState.ATTACHED);
	}

	public void Select(bool val)
	{
		if( val )
		{
			machine.SetState(PartState.SELECTED);		
		}
		else
		{
			machine.SetState(PartState.ATTACHED);
		}
	}

	public void OnChange(int prevIdx)
	{
		Color color = Color.white;
		switch(machine.GetActiveState())
		{
			case 0: color = Color.white; break;
			case 1: color = thisBody.mainColor; break;
			case 2: color = selectedColor; break;
			case 3: color = Color.red; break;
			case 4: color = Color.magenta; break;
			case 5: color = Color.cyan; break;
			case 6: color = Color.yellow; break;
			case 7: color = Color.white; break;
		}
		debugState = (PartState)machine.GetActiveState();
		//PartState lastState = (PartState)prevIdx;
		SetColor(color);		
	}

	public void SetColor(Color color)
	{
		thisRenderer.material.SetColor("_Color",color);
		//thisRenderer.material.SetColor("_EmissionColor",color);
	}

	public void OnCollisionEnter(Collision collision)
	{
		if( collision.relativeVelocity.sqrMagnitude < 3f*3f)
		{
			return;
		}
		ActorBodyPart bodyPart = collision.collider.GetComponent<ActorBodyPart>();
		if( bodyPart != null )
		{
			if( bodyPart.thisBody == thisBody )
			{
				return;	
			}
			else
			{
				bodyPart.HitBy(this);
				this.HitBy(bodyPart);
			}
		}
		else
		{
			//wall or ground?
			if( glueState == GlueState.LIMB || glueState == GlueState.FREE )
			{
				Expire();
			}
			else
			{

			}
		}
		
	}

	public void HitBy(ActorBodyPart other)
	{
		//Debug.Log(this+"hit by "+other+" glueState"+glueState);
		Expire();
	}
}
