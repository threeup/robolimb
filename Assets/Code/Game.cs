using UnityEngine;
using System.Collections;
using BasicCommon;

public class Game : MonoBehaviour 
{

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
}
