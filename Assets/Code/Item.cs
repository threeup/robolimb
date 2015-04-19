using UnityEngine;
using System.Collections;
using BasicCommon;


public class Item : MonoBehaviour 
{
	public void Spawn()
	{
		Game.Instance.Register(this);
	}
}
