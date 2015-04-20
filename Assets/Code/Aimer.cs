using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BasicCommon;


public class Aimer : MonoBehaviour 
{
	public Actor thisActor;
	public List<GameObject> aimChildren;
	public float spacing = 1f;
	bool aimEnabled = true;
	public bool canDraw = true;

	public GameObject aimTarget;

	void Awake()
	{
		SetEnabled(false);
	}

	public void Update()
	{
		if( aimEnabled )
		{
			TargetForwardest();
		}
		else
		{
			aimTarget = null;
		}
	}

	void TargetForwardest()
	{
		List<Actor> livingActors = Game.Instance.livingActors;
        float bestDot = 0f;
        Actor bestActor = null;
        foreach(Actor otherActor in livingActors)
        {
			if( otherActor.team != thisActor.team )
            {
				Vector3 forward = thisActor.transform.forward;
            	Vector3 toOther = (otherActor.transform.position - thisActor.transform.position).normalized;
                float dot = Vector3.Dot(forward, toOther);
				Debug.DrawRay(thisActor.transform.position, forward*10f, Color.white, 1f);
				Debug.DrawRay(thisActor.transform.position, toOther*10f, Color.green, 1f);
				Debug.Log("dot to"+otherActor+" "+dot+" "+toOther);
                if( dot > bestDot )
                {
                    bestDot = dot;
					bestActor = otherActor;
                }
            }
        }
        if( bestActor != null && aimTarget != bestActor.gameObject)
        {
            aimTarget = bestActor.gameObject;
            Debug.Log("new aim target");
        }
	}

	public void SetEnabled(bool val)
	{
		if( aimEnabled == val )
		{
			return;
		}
		aimEnabled = val;
		if( val == false || canDraw )
		{
			for(int i=0; i<aimChildren.Count; ++i)
			{
				aimChildren[i].SetActive(val);
			}	
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
