using UnityEngine;
using System.Collections;
using BasicCommon;
using UnityStandardAssets.Characters.ThirdPerson;

public class Spawner : MonoBehaviour 
{
	public GameObject prefab;

	public void Start()
	{
		Game.Instance.Register(this);
	}

	public Actor Spawn(bool isAI)
	{
		GameObject go = Instantiate(prefab, this.transform.position, this.transform.rotation) as GameObject;
		Actor spawn = go.GetComponent<Actor>();
		ThirdPersonUserControl user = go.GetComponent<ThirdPersonUserControl>();
		user.Spawn(isAI);
		return spawn;
	}
}
