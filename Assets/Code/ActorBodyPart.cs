using UnityEngine;
using System.Collections;
using DG.Tweening;
using BasicCommon;

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
	LegLeftLower = 21,
	LegRightUpper = 22,
	LegRightLower = 23,
	None = 99,
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
	private Transform nextParent;

	private BasicTimer colliderTimer = new BasicTimer(0f);
	private BasicTimer expireTimer = new BasicTimer(0f);

	public int depth = -1;


	void Awake()
	{
		thisTransform = this.transform;
		thisRenderer = this.GetComponent<Renderer>();
		originalParent = thisTransform.parent;
		originalPos = thisTransform.localPosition;
		originalRot = thisTransform.localRotation;
		originalScale = thisTransform.localScale;
		CheckDepth();
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
		}
		if( expireTimer.Tick(deltaTime) )
		{
			Expire();
		}
	}

	public void AlignTo(ActorBodyPart other)
	{
		ParentChildren();
		nextParent = other.transform;
		thisTransform.SetParent(thisBody.skeleton.transform, true);
		thisTransform.DOMove(other.transform.position, 1).OnComplete(Reparent);
	}

	public void Expire()
	{
		if( childPart != null )
		{
			childPart.Expire();
		}
		GameObject debris = Instantiate(this.gameObject, thisTransform.position, thisTransform.rotation) as GameObject;
		debris.transform.localScale = originalScale;
		ActorBodyPart debrisPart = debris.GetComponent<ActorBodyPart>();
		Destroy(debrisPart);

		thisTransform.SetParent(originalParent);
		thisTransform.localPosition = originalPos;
		thisTransform.localRotation = originalRot;
		thisTransform.localScale = 0.0001f*Vector3.one;
		thisCollider.enabled = false;
		thisRigidbody.isKinematic = true;
	}

	public void Grow()
	{
		thisTransform.DOScale(originalScale, 1f).OnComplete(Reregister);	
	}

	public void ParentChildren()
	{
		if( childPart != null ) 
		{
			childPart.transform.SetParent(this.transform, true);
			childPart.ParentChildren();
		}
	}

	void Reparent()
	{
		thisTransform.SetParent(nextParent, true);
		thisTransform.DOLocalMove(Vector3.zero, 0.2f);
	}

	public void Animate()
	{

	}
	public void Launch(float force, Vector3 direction)
	{
		Select(false);
		thisTransform.SetParent(null, true);
		thisRigidbody.isKinematic = false;
		thisRigidbody.AddForce(force*direction);
		colliderTimer = new BasicTimer(0.2f, false);
		expireTimer = new BasicTimer(1f, false);
	}

	public void Reregister()
	{
		thisBody.Reregister(this);
	}

	public void Select(bool val)
	{
		thisRenderer.material.color = val ? Color.green : Color.blue;
	}
}
