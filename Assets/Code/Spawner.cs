using UnityEngine;
using System.Collections;
using BasicCommon;
using UnityStandardAssets.Characters.ThirdPerson;

public enum SpawnType
{
	ACTOR_PC,
	ACTOR_AI,
	ITEM,
}
public class Spawner : MonoBehaviour 
{
	public ActorTeam team;
	public SpawnType spawnType;
	public GameObject prefab;
	public bool selfSpawn = false;
	public float spawnInterval = 1f;
	public SphereCollider thisCollider;
	public LayerMask collisionMask;
	GameObject spawned = null;
	BasicTimer spawnTimer = new BasicTimer(0);


	public void Start()
	{
		Game.Instance.Register(this);
		spawnTimer = new BasicTimer(spawnInterval, true);
		spawnTimer.Duration = spawnInterval*UnityEngine.Random.Range(0.5f,2f);
		spawnTimer.Reset();
	}

	bool CanSpawn()
	{
		return selfSpawn && (spawned == null || spawned.activeSelf == false);
	}

	public void Update()
	{
		float deltaTime = Time.deltaTime;
		if(CanSpawn() && spawnTimer.Tick(deltaTime))
		{
			Spawn();
			spawnTimer.Duration = spawnInterval*UnityEngine.Random.Range(0.5f,2f);
			spawnTimer.Reset();
		}
	}
	public GameObject Spawn()
	{
		Collider[] hit = Physics.OverlapSphere(this.transform.position+thisCollider.center, thisCollider.radius, collisionMask);
		if( hit.Length > 0 )
		{
			//Debug.Log("Blocked"+hit[0]);
			return null;
		}

		spawned = Instantiate(prefab, this.transform.position, this.transform.rotation) as GameObject;
		ThirdPersonUserControl user = spawned.GetComponent<ThirdPersonUserControl>();
		Actor actor = spawned.GetComponent<Actor>();
		Item item = spawned.GetComponent<Item>();
		switch(spawnType)
		{
			case SpawnType.ACTOR_PC:
				user.Spawn(true);
				actor.Spawn(team, true);
				break;
			case SpawnType.ACTOR_AI:
				user.Spawn(false);
				actor.Spawn(team, false);
				break;
			default:
			case SpawnType.ITEM:
				item.Spawn();
				break;
		}
		return spawned;
	}
}
