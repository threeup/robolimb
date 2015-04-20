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

	public ActorBody body;
	public Aimer aimer;

	public BasicTimer throwTimer = new BasicTimer(0);
	public BasicTimer superTimer = new BasicTimer(0);
	public bool aiThrow;

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
		if( aimer != null )
		{
			if( throwPhase == ThrowPhase.CHARGE )
			{
				aimer.SetEnabled(true);
				aimer.SetAimAmount(1f-throwTimer.Percent);
				if( aimer.aimTarget != null )
				{
					aimer.transform.LookAt(aimer.aimTarget.transform, Vector3.up);
				}
			}
			else
			{
				aimer.SetEnabled(false);
			}
		}
	}

	public Vector3 ThrowVector()
	{
		Vector3 throwVec = 0.7f*aimer.transform.forward+0.3f*this.transform.up;
		return throwVec.normalized;

	}

	public void GoSuper()
	{
		body.superTimer = new BasicTimer(2f, false);
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
				else
				{
					TryCycle();
				}
				break;
			case ThrowPhase.ALIGN: 
				throwTimer = new BasicTimer(6f, false);
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

	public void AIThrow(bool val)
	{
		aiThrow = val;
		Throwing(val, false);
	}
}
