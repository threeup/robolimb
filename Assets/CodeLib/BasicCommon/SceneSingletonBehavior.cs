using System;
using UnityEngine;

public class SceneSingletonBehavior<T> : MonoBehaviour where T : MonoBehaviour
{
	protected static T _instance;
	protected static bool appShuttingDown = false;
	
	public static T Instance
	{
		get
		{
			if( _instance == null )
			{
				if (appShuttingDown)
					return null;
				
				_instance = (T) FindObjectOfType(typeof(T));
				if( _instance == null )
				{
					// Changed this to a log so that the engineer can capture and manage the null if desired without halting the program. CA - 01/02/2013
					Debug.LogError("Instance of " + typeof(T) + " is required, but not found in the scene.");
					//throw new NullReferenceException("Instance of " + typeof(T) + " is required, but not found in the scene.");
				}
			}
			
			return _instance;
		}
	}
	
	public static bool HasInstance()
	{
		return _instance != null;
	}
    public virtual void OnDestroy()
    {
        if (this == _instance)
            _instance = null;
    }
	
	void OnApplicationQuit()
	{
		appShuttingDown = true;	
	}
}


