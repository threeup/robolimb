using System;
using System.Collections.Generic;
using UnityEngine;

// KeyMessenger has T and U,  T is the key and U is the argument structure
// For generic events, T will be a string and U will be GenericArgs
// For specific events T will be the sender and U will be a specific struct
// The handler must follow the delegate inside angle brackets 

// Generic Example:
//		GenericArgs list = new GenericArgs(activatingObject.GetComponent<Actor>(), this.gameObject, null);
//		KeyMessenger<string, GenericArgs>.Invoke(GenericEvents.EVENT_ITEMINTERACTION, list);
//
//      KeyMessenger<string, GenericArgs>.AddKeyListener(GenericEvents.EVENT_ITEMINTERACTION, Evaluate);



// Specific Example:

//  This listens to global noises
//      KeyMessenger<AINoise, AINoise_Detection_Event>.Invoke(noise, new AINoise_Detection_Event{ isCreated = true });
//      KeyMessenger<AINoise, AINoise_Detection_Event>.AddGlobalListener(detectNoise)
// 
//  This listens to only local player activestate changes
//      KeyMessenger<Actor, ActiveArgs>.Invoke(this, new ActiveArgs{ isActive = true });
//      KeyMessenger<Actor, ActiveArgs>.AddKeyListener(player, ActorActiveChanged);
// 
//  Complicated event arguments
//      Actor_FinishAction_Event usedArgs = new Actor_FinishAction_Event(ability, source, null, item, target, ability.offensiveAbility);
//      KeyMessenger<Actor, Actor_FinishAction_Event>.Invoke(player, usedArgs);


static public class KeyBoss
{
	public static Dictionary<string, int> typeCount = new Dictionary<string, int>();

	public static void Output()
	{
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		foreach(string t in typeCount.Keys)
		{
			sb.Append(t+"="+typeCount[t]+", ");
		}
	}
}


static public class KeyMessenger<T, U>
{
	// T arg1 is the key
	// U arg2 is a struct, GenericArgs, or a specific eventargs
    public delegate void KeyDelegate(T arg1, U arg2);

    // global
    private static List<KeyDelegate> globalTable = new List<KeyDelegate>();
    
    
    static public void AddGlobalListener(KeyDelegate handler)       
    {
        globalTable.Add(handler);

        //KeyBoss.typeCount[typeof(T).ToString()+"+"+typeof(U).ToString()] = globalTable.Count;
		//KeyBoss.Output();

    }
    static public void RemoveGlobalListener(KeyDelegate handler)       
    {
        globalTable.Remove(handler);

        //KeyBoss.typeCount[typeof(T).ToString()+"+"+typeof(U).ToString()] = globalTable.Count;
		//KeyBoss.Output();
	
    }

    // selective
	private static Dictionary<T, KeyDelegate> selectiveTable = new Dictionary<T, KeyDelegate>();
    

    static public void ClearLocalEvents(T key)
    {
		selectiveTable.Remove(key);
    }

    static public void ClearAllEvents()
    {
        selectiveTable.Clear();
        globalTable.Clear();
    }

    static public void AddKeyListener(T key, KeyDelegate handler)       
    {
    	lock (selectiveTable)
    	{
    		KeyDelegate d;
	      	if (selectiveTable.TryGetValue(key, out d))
	      	{
	      		selectiveTable[key] = Delegate.Combine(d, handler) as KeyDelegate;
	      	}
	      	else
	      	{
	      		selectiveTable[key] = handler;
	      	}
	      	
	      	//KeyBoss.typeCount[typeof(T).ToString()+"+"+typeof(U).ToString()] = selectiveTable.Count;
			//KeyBoss.Output();

	    }
    }
    static public void RemoveKeyListener(T key, KeyDelegate handler)       
    {
    	lock (selectiveTable)
    	{
	        KeyDelegate d;
			if (selectiveTable.TryGetValue(key, out d)) 
			{
				d = Delegate.Remove(d, handler) as KeyDelegate;
				selectiveTable[key] = d;
				if (selectiveTable[key] == null)
                {
                    selectiveTable.Remove(key);
                }
			}
			
			//KeyBoss.typeCount[typeof(T).ToString()+"+"+typeof(U).ToString()] = selectiveTable.Count;
			//KeyBoss.Output();

		}
	}

	// everything
	static public void Invoke(T arg1, U arg2)
	{
		if(arg1 is string)
		{
			//Logger.Log(LogChannel.TRIG, LogLevel.Debug, "Trigger Event <"+arg1.ToString()+"> fired");
		}
		lock (globalTable)
		{
			// backwards to allow for selfdeletion
			for(int i = globalTable.Count-1; i>=0; --i)
			{
				KeyDelegate h = globalTable[i];
				h(arg1, arg2);
			}
		}

		lock (selectiveTable)
    	{
    		KeyDelegate d;
			if (selectiveTable.TryGetValue(arg1, out d)) 
			{
				d(arg1, arg2);
			}
		}
	}

	static public int Count(T arg1)
	{
		lock (selectiveTable)
    	{
    		KeyDelegate d;
			if (selectiveTable.TryGetValue(arg1, out d)) 
			{
				return d.GetInvocationList().Length;
			}
		}
		return 0;
	}

    static public int CountAll()
    {
        int count = 0;
        lock (selectiveTable)
        {
          
            foreach (T arg in selectiveTable.Keys)
            {
                KeyDelegate d;
                if (selectiveTable.TryGetValue(arg, out d))
                {
                    count += d.GetInvocationList().Length;
                }
            }
           
        }

        return count + globalTable.Count;
    }

	static Queue<KeyValuePair<T,U>> invokeQueue;

	static public void DelayedInvoke(T arg1, U arg2)
	{
		if (invokeQueue == null)
		{
			invokeQueue = new Queue<KeyValuePair<T,U>>();
		}
		invokeQueue.Enqueue(new KeyValuePair<T,U>(arg1, arg2));
	}

	static public void Update()
	{
		while(invokeQueue != null && invokeQueue.Count > 0)
		{
			KeyValuePair<T,U> kvp = invokeQueue.Dequeue();
			Invoke(kvp.Key, kvp.Value);
		}
	}
}