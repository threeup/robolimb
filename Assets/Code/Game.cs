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

	public GameObject canvas;

	void Awake()
	{
		Instance = this;
		machine = new BasicMachine<GameState>();
		machine.Initialize(typeof(GameState));
		machine[(int)GameState.INTRO].OnEnter = OnIntro;
		machine[(int)GameState.INTRO].OnExit = OnIntroExit;
		machine[(int)GameState.SPAWNING].OnEnter = OnSpawn;
		machine[(int)GameState.SPAWNING].CanEnter = CanSpawn;
		machine[(int)GameState.ACTION].CanEnter = CanAction;
	}



	public void Update()
	{
		if( machine.failedState != null)
		{
			machine.RetryFailedState();
		}
	}


	float lastInput = 0f;
	 private void FixedUpdate()
    {
        bool acceptingInput = Time.time - lastInput > 0.35f;
        if( acceptingInput )
        {
        	GameState currentState = (GameState)machine.GetActiveState();
	        bool speedDown = Input.GetKey(KeyCode.F1);
	        bool speedUp = Input.GetKey(KeyCode.F2);
	        bool kill = Input.GetKey(KeyCode.Escape);
	        bool start = Input.GetKey(KeyCode.Space);
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
	        if( start && currentState == GameState.INTRO)
	        {
	        	machine.SetState(GameState.SPAWNING);
	        }
	        if( kill && currentState == GameState.INTRO)
	        {
	        	Application.Quit();
	        }
	        if( kill && currentState == GameState.ACTION)
	        {
	        	machine.SetState(GameState.INTRO);
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
		actor.name = "Actor"+GetNextUID();
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
	public void Deregister(Actor actor)
	{
		if( livingActors.Contains(actor) )
		{
			livingActors.Remove(actor);
		}
	}
    public void Deregister(Item item)
	{
		if( livingItems.Contains(item) )
		{
			livingItems.Remove(item);
		}
	}

	static int nextUID = 1;
	int GetNextUID()
	{
		int result = nextUID++;
		return result;
	}


	bool CanSpawn()
	{
		return spawners.Count > 3;
	}

	bool CanAction()
	{
		return livingActors.Count > 1;
	}

	public void OnIntro()
	{
		canvas.SetActive(true);
		foreach(Actor actor in livingActors)
		{
			Destroy(actor.gameObject);
		}
		livingActors.Clear();
		foreach(Item item in livingItems)
		{
			Destroy(item.gameObject);
		}
		livingItems.Clear();
	}

	public void OnIntroExit()
	{
		canvas.SetActive(false);
	}

	public void OnSpawn()
	{
		int alivePC = 0;
		int aliveAI = 0;
		int desiredPC = 1;
		int desiredAI = 5;
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
				if( go != null )
				{
					autoCam.SetTarget(go.transform);
					alivePC++;
				}
			}
			else if(aliveAI < desiredAI)
			{
				spawner.spawnType = SpawnType.ACTOR_AI;
				go = spawner.Spawn();
				if( go != null )
				{
					aliveAI++;
				}
			}
		}
		machine.SetState(GameState.ACTION);
	}
}
