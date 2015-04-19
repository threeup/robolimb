using UnityEngine;
using System.Collections;
using BasicCommon;


public class Actor : MonoBehaviour 
{

	public enum ThrowPhase
	{
		NONE,
		ALIGN,
		CHARGE,
		FLY
	}
	public ActorTeam team = ActorTeam.NONE;
	public ThrowPhase throwPhase = ThrowPhase.NONE;

	public float headDistance = 0f;
	public ActorBody body;
	public BasicTimer throwTimer = new BasicTimer(0);

	float lastCycle = 0f;
	// Use this for initialization
	void Start () 
	{
	
	}

	public void Spawn(ActorTeam team)
	{
		this.team = team;
		switch(team)
		{
			case ActorTeam.RED: body.Spawn(Color.red); break;
			case ActorTeam.BLUE: body.Spawn(Color.blue); break;
			case ActorTeam.NONE: body.Spawn(Color.grey); break;
		}
		Game.Instance.Register(this);
	}
	
	// Update is called once per frame
	void Update () 
	{
		float deltaTime = Time.deltaTime;
		if( throwTimer.Tick(deltaTime) )
		{
			AdvanceThrow();
		}
	}

	public void AdvanceThrow()
	{
		switch(throwPhase)
		{
			case ThrowPhase.NONE:
				if( body.CanThrowStart() )
				{
					throwTimer = new BasicTimer(1f, false);
					body.AlignToThrower();
					throwPhase = ThrowPhase.ALIGN;
				}
				break;
			case ThrowPhase.ALIGN: 
				throwTimer = new BasicTimer(20f, false);
				body.Animate(); 
				throwPhase = ThrowPhase.CHARGE;
				break;
			case ThrowPhase.CHARGE: 
				if( body.CanThrowFinish() )
				{
					body.Launch(1f-throwTimer.Percent);
					throwTimer.Pause(true);
					throwPhase = ThrowPhase.NONE;
				}
				break;
			default: 
				break;
		}
	}


	public void Throwing(bool isThrowing, bool isCycling)
	{
		switch(throwPhase)
		{
			case ThrowPhase.NONE:
				if( isThrowing )
				{
					AdvanceThrow();
				}
				if( isCycling )
				{
					TryCycle();
				}
				break;
			case ThrowPhase.ALIGN:
				//nothing
				break;
			case ThrowPhase.CHARGE:
				if( !isThrowing )
				{
					AdvanceThrow();
				}
				break;
		}
	}

	public void TryCycle()
	{
		float timeSince = Time.time - lastCycle;
		if( timeSince > 0.25f )
		{
			body.Cycle();
			lastCycle = Time.time;
		}
	}
}
