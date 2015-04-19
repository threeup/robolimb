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

public enum ActorTeam
{
	NONE,
	RED,
	BLUE,
}

public class Game : MonoBehaviour 
{
	public static Game Instance;
	List<Spawner> spawners = new List<Spawner>();
	public List<Actor> livingActors = new List<Actor>();
	public List<Item> livingItems = new List<Item>();
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
    public void Register(Actor actor)
	{
		if( !livingActors.Contains(actor) )
		{
			livingActors.Add(actor);
		}
	}
    public void Register(Item item)
	{
		if( !livingItems.Contains(item) )
		{
			livingItems.Add(item);
		}
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
			if( spawner.selfSpawn )
			{
				continue;
			}
			GameObject go = null;
			if(alivePC < desiredPC)
			{
				spawner.spawnType = SpawnType.ACTOR_PC;
				go = spawner.Spawn();
				autoCam.SetTarget(go.transform);
				alivePC++;
			}
			else
			{
				spawner.spawnType = SpawnType.ACTOR_AI;
				go = spawner.Spawn();
			}
		}
	}
}
