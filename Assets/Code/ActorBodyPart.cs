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
	LAUNCH,
	GLUED,
	DORMANT,
	GROWING,
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

	private Transform originalParent;
	private Vector3 originalPos;
	private Quaternion originalRot;
	private Vector3 originalScale;
	public Transform nextParent;

	private BasicTimer colliderTimer = new BasicTimer(0f);
	private BasicTimer expireTimer = new BasicTimer(0f);

	public BasicMachine<PartState> machine;

	public int depth = -1;

	public bool stateLock = false;
	private PartState debugState;

	private Vector3 nextForce = Vector3.zero;

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
		machine[(int)PartState.LAUNCH].OnEnter = OnLaunch;
		machine[(int)PartState.LAUNCH].CanEnter = CanSwitch;
		machine[(int)PartState.DORMANT].OnEnter = OnExpire;
		machine[(int)PartState.DORMANT].CanEnter = CanSwitch;
		machine[(int)PartState.GROWING].OnEnter = OnGrow;
		machine[(int)PartState.GROWING].CanEnter = CanSwitch;

		thisTransform = this.transform;
		thisRenderer = this.GetComponent<Renderer>();
		thisBody = this.GetComponentInParent<ActorBody>();
		originalParent = thisTransform.parent;
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
	public bool CanWeaponDeselect()
	{
		return machine.IsState(PartState.SELECTED) || machine.IsState(PartState.LAUNCH);
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
		if( colliderTimer.Tick(deltaTime) )
		{
			thisCollider.enabled = true;
			StateLock(false);
		}
		if( expireTimer.Tick(deltaTime) )
		{
			machine.SetState(PartState.DORMANT);
		}
	}

	void StateLock(bool val)
	{
		stateLock = val;
		if( !val )
		{
			bool? unlockResult = machine.SetFailedState();
			if( unlockResult != null )
			{
				PartState st = (PartState)machine.GetActiveState();
			}
		}
		
	}

	void OnAttach()
	{
		thisBody.Register(this);
	}

	void OnAlign()
	{
		Glue(true);
		SetParent(thisBody.skeleton.transform);
		StateLock(true);
		thisTransform.DOMove(nextParent.position, 1).OnComplete(SetStateHeld);
	}

	void SetStateHeld()
	{
		StateLock(false);
		machine.SetState(PartState.HELD);
	}

	void OnExpire()
	{
		Glue(false);

		GameObject debris = Instantiate(this.gameObject, thisTransform.position, thisTransform.rotation) as GameObject;
		debris.transform.localScale = originalScale;
		ActorBodyPart debrisPart = debris.GetComponent<ActorBodyPart>();
		//debrisPart.thisRenderer.material = new Material(thisRenderer.material);
		debrisPart.thisRenderer.material.color = Color.green;
		//debrisPart.SetParent(thisTransform.parent);
		Destroy(debrisPart);

		SetParent(originalParent);
		thisTransform.localPosition = originalPos;
		thisTransform.localRotation = originalRot;
		thisTransform.localScale = 0.0001f*Vector3.one;
		thisCollider.enabled = false;
		thisRigidbody.isKinematic = true;

		thisBody.Dormant(this);
		
	}

	void SetParent(Transform nextParent)
	{
		thisTransform.SetParent(nextParent, true);
	}

	void OnGrow()
	{
		StateLock(true);
		thisTransform.DOScale(originalScale, 1f).OnComplete(Reregister);	
	}

	public void Glue(bool val)
	{
		if( childPart != null ) 
		{
			if( val )
			{
				childPart.SetParent(this.transform);
				childPart.machine.SetState(PartState.GLUED);
			}
			else
			{
				childPart.SetParent(null);	
				childPart.machine.SetState(PartState.DORMANT);
			}
			childPart.Glue(val);
		}
	}

	void OnHeld()
	{
		SetParent(nextParent);
		StateLock(true);
		thisTransform.DOLocalMove(Vector3.zero, 0.2f).OnComplete(Unlock);
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
		SetParent(null);

		//test
		//thisTransform.position = thisTransform.position + Vector3.one*10f;
		thisRigidbody.isKinematic = false;
		thisRigidbody.AddForce(nextForce);
		
		StateLock(true);
		thisCollider.enabled = false;
		colliderTimer = new BasicTimer(0.2f, false);
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
			case 0: color = Color.black; break;
			case 1: color = Color.blue; break;
			case 2: color = Color.green; break;
			case 3: color = Color.red; break;
			case 4: color = Color.magenta; break;
			case 5: color = Color.cyan; break;
			case 6: color = Color.yellow; break;
			case 7: color = Color.white; break;
		}
		debugState = (PartState)machine.GetActiveState();
		//PartState lastState = (PartState)prevIdx;
		//Debug.Log("Change"+lastState+" "+debugState);
		thisRenderer.material.color = color;
	}
}
