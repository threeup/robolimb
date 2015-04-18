using System;
using UnityEngine;

public class LazySingletonBehavior<T> : MonoBehaviour where T : MonoBehaviour
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
					GameObject go = new GameObject(typeof(T).ToString());						
					_instance = (T)go.AddComponent<T>();
				
					if( _instance == null )
					{
						Debug.LogError("Instance of " + typeof(T) + " is required, could not be found or created");
					}
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