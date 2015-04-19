using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BasicCommon;
using GumboLib;
using UnityStandardAssets.Cameras;

public enum GameState
{
	INTRO,
	SPAWNING,
	ACTION,
	FINISHED
}


public class Game : MonoBehaviour 
{
	public static Game Instance;
	List<Spawner> spawners = new List<Spawner>();
	public BasicMachine<GameState> machine;
	public AutoCam autoCam;


	void Awake()
	{
		Instance = this;
		machine = new BasicMachine<GameState>();
		machine.Initialize(typeof(GameState));
		machine[(int)GameState.SPAWNING].OnEnter = OnSpawn;
		machine[(int)GameState.SPAWNING].CanEnter = CanSpawn;
	}



	public void Update()
	{
		switch(machine.GetActiveState())
		{
			case (int)GameState.INTRO:
				if( Input.GetKey(KeyCode.Space) )
				{
					machine.SetState(GameState.SPAWNING);
				}
				break;
			default:
				break;
		}
	}


	float lastInput = 0f;
	 private void FixedUpdate()
    {
        bool acceptingInput = Time.time - lastInput > 0.35f;
        if( acceptingInput )
        {
	        bool speedDown = Input.GetKey(KeyCode.F1);
	        bool speedUp = Input.GetKey(KeyCode.F2);
	        float nextTimeScale = 0f;
	        if( speedUp )
	        {
	        	nextTimeScale = Time.timeScale + 0.2f;
	        }
	        if( speedDown )
	        {
	        	nextTimeScale = Time.timeScale - 0.2f;
	        }

	        if( speedDown || speedUp )
	        {
	        	Time.timeScale = Mathf.Clamp(nextTimeScale, 0.2f, 3f);
	        	lastInput = Time.time;
	        	Debug.Log("nextTimeScale"+nextTimeScale);
	        }
	    }
    }

    public void Register(Spawner spawner)
	{
		if( !spawners.Contains(spawner) )
		{
			spawners.Add(spawner);
		}
		machine.RetryFailedState();

	}

	bool CanSpawn()
	{
		return spawners.Count > 3;
	}

	public void OnSpawn()
	{
		int alivePC = 0;
		int desiredPC = 1;
		foreach(Spawner spawner in spawners)
		{
			if(alivePC < desiredPC)
			{
				Actor actor = spawner.Spawn(false);
				autoCam.SetTarget(actor.transform);
				alivePC++;
			}
			else
			{
				spawner.Spawn(true);
			}
		}
	}
}
