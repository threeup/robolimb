using UnityEngine;
using System.Collections;

public enum BodyPartType
{
	Head,
	ArmLeftUpper,
	ArmLeftLower,
	ArmRightUpper,
	ArmRightLower,
	LegLeftUpper,
	LegLeftLower,
	LegRightUpper,
	LegRightLower,
}

public class ActorBodyPart : MonoBehaviour 
{
	public Rigidbody thisRigidbody;
	public Collider thisCollider;
	public BodyPartType bodyPartType;


	public void AlignTo(ActorBodyPart other)
	{
		this.transform.parent = other.transform;
	}

	public void Animate()
	{

	}
	public void Launch(float speed, Vector3 direction)
	{
		thisRigidbody.isKinematic = true;
		thisRigidbody.AddForce(speed*direction);
	}
}
