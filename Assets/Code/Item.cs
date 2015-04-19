using UnityEngine;
using System.Collections;
using BasicCommon;


public class Item : MonoBehaviour 
{
	public void Spawn()
	{
		Game.Instance.Register(this);
	}

	public void OnTriggerEnter(Collider other)
	{
		Actor actor = other.GetComponent<Actor>();
		if( actor != null )
		{
			actor.GoSuper();
			Game.Instance.Deregister(this);
			Destroy(this.gameObject);
		}
	}
}
