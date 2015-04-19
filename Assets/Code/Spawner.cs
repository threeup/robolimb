using UnityEngine;
using System.Collections;
using BasicCommon;
using UnityStandardAssets.Characters.ThirdPerson;

public enum SpawnType
{
	ACTOR_PC,
	ACTOR_AI,
	POWERUP,
}
public class Spawner : MonoBehaviour 
{
	public SpawnType spawnType;
	public GameObject prefab;
	public bool selfSpawn = false;
	public float spawnInterval = 1f;
	GameObject spawned = null;
	BasicTimer spawnTimer = new BasicTimer(0);


	public void Start()
	{
		Game.Instance.Register(this);
		spawnTimer = new BasicTimer(spawnInterval, true);
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
		}
	}
	public GameObject Spawn()
	{
		spawned = Instantiate(prefab, this.transform.position, this.transform.rotation) as GameObject;
		ThirdPersonUserControl user = spawned.GetComponent<ThirdPersonUserControl>();;
		switch(spawnType)
		{
			case SpawnType.ACTOR_PC:
				user.Spawn(true);
				break;
			case SpawnType.ACTOR_AI:
				user.Spawn(false);
				break;
			default:
			case SpawnType.POWERUP:
				break;
		}
		return spawned;
	}
}
