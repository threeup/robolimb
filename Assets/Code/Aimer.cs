using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BasicCommon;


public class Aimer : MonoBehaviour 
{

	public List<GameObject> aimChildren;
	public float spacing = 1f;
	bool aimEnabled = true;

	void Awake()
	{
		SetEnabled(false);
	}

	public void SetEnabled(bool val)
	{
		if( aimEnabled == val )
		{
			return;
		}
		aimEnabled = val;
		for(int i=0; i<aimChildren.Count; ++i)
		{
			aimChildren[i].SetActive(val);
		}	
	}

	public void SetAimAmount(float val)
	{

		for(int i=0; i<aimChildren.Count; ++i)
		{
			Vector3 pos = Vector3.forward*i*spacing;
			pos.y = i*val;
			aimChildren[i].transform.localPosition = pos;
		}
	}
}
