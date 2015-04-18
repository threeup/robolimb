using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BasicCommon;


public enum ThrowPhase
{
	NONE,
	ALIGN,
	CHARGE,
	FLY
}

public class ActorBody : MonoBehaviour 
{

	public List<ActorBodyPart> bodyParts;

	public BasicTimer throwTimer = new BasicTimer(5);

	public ThrowPhase throwPhase;

	public BodyPartType currentLaunchingPart = BodyPartType.ArmLeftLower;
	public BodyPartType currentThrowingPart = BodyPartType.ArmRightLower;

	public float currentSpeed = 10f;
	public Vector3 currentDirection = Vector3.zero;

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		float deltaTime = Time.deltaTime;
		if( throwTimer.Tick(deltaTime) )
		{
			AdvanceThrow();
		}
		currentDirection = this.transform.forward;
	}

	public void AdvanceThrow()
	{
		switch(throwPhase)
		{
			case ThrowPhase.NONE: AlignToThrower(currentLaunchingPart, currentThrowingPart); break;;
			case ThrowPhase.ALIGN: Animate(currentThrowingPart); break;
			case ThrowPhase.CHARGE: Launch(currentLaunchingPart); break;
			default: break;
		}
	}

	public void AlignToThrower(BodyPartType launch, BodyPartType thrower)
	{
		ActorBodyPart destination = bodyParts.Find(x=>x.bodyPartType == thrower);
		foreach(ActorBodyPart bp in bodyParts)
		{
			if( bp.bodyPartType == launch )
			{
				bp.AlignTo(destination);
			}
		}
	}

	public void Animate(BodyPartType thrower)
	{
		foreach(ActorBodyPart bp in bodyParts)
		{
			if( bp.bodyPartType == thrower )
			{
				bp.Animate();
			}
		}
	}

	public void Launch(BodyPartType launch)
	{
		foreach(ActorBodyPart bp in bodyParts)
		{
			if( bp.bodyPartType == launch )
			{
				bp.Launch(currentSpeed, currentDirection);
			}
		}
	}
}
