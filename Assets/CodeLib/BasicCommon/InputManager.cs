using System;
using System.Collections.Generic;
using UnityEngine;

namespace BasicCommon 
{

    public enum InputAxis
    {
    	MAIN_X,
    	MAIN_Y,
    	MOUSE_X,
    	MOUSE_Y,
    	MOUSEWHEEL_Y,
    };

    public class InputManager : LazySingletonBehavior<InputManager>
    {
    	public int debugConsumedInputFrame = 0;
    	public bool debugConsumedInputLock = false;
        bool consumeInputKeyDownLock = false;

    	public int bindCount = 0;
    //	public string bindString = "";
    //	public string unbindString = "";
    	public bool hasShift = false;
    	public bool hasAlt = false;
    	public bool hasCtrl = false;
    	public bool hasCommand = false;

    	public string[] axisNames = new string[]{"Horizontal", "Vertical", "Mouse X", "Mouse Y", "Mouse ScrollWheel"};
    	public delegate void InputDelegate();
    	public delegate void AxisDelegate(float val);

        public bool ConsumedInput { get { return debugConsumedInputLock || consumeInputKeyDownLock || debugConsumedInputFrame > 0; } }

    	private Dictionary<KeyCode, InputDelegate> keyDownTable = new Dictionary<KeyCode, InputDelegate>();
    	private Dictionary<KeyCode, InputDelegate> keyUpTable = new Dictionary<KeyCode, InputDelegate>();
    	private Dictionary<int, AxisDelegate> axisTable = new Dictionary<int, AxisDelegate>();
     
     	public void Awake()
     	{
     		hasShift = false;
    		hasAlt = false;
    		hasCtrl = false;
    		hasCommand = false;

    	    GameObject.DontDestroyOnLoad(this);
            DontDestroyOnLoad(this.gameObject);
     	}

    	public static void Bind(KeyCode key, bool isDown, InputDelegate handler)
    	{
    		Instance.AddKeyBind(key, isDown, handler);
    	}
    	public static void Unbind(KeyCode key, bool isDown, InputDelegate handler)
    	{
    		if (HasInstance())
    		{
    			Instance.RemoveKeyBind(key, isDown, handler);
    		}
    	}

    	public static void Bind(int axis, AxisDelegate handler)
    	{
    		Instance.AddAxisBind(axis, handler);
    	}
    	public static void Unbind(int axis, AxisDelegate handler)
    	{
    		if (HasInstance())
    		{
    			Instance.RemoveAxisBind(axis, handler);
    		}
    	}

    	public void AddKeyBind(KeyCode key, bool isDown, InputDelegate handler)
    	{
    		InputDelegate deleg;
    		var inputTable = isDown ? keyDownTable : keyUpTable;
    		if (inputTable.TryGetValue(key, out deleg))
          	{
          		inputTable[key] = Delegate.Combine(deleg, handler) as InputDelegate;
    			//UnityEngine.Debug.LogError(key+" leaking invok list"+inputTable[key].GetInvocationList().Length);
          	}
          	else
          	{
          		inputTable[key] = handler;
          	}
          	bindCount++;
          	//bindString+= key.ToString();
    	}
    	public void RemoveKeyBind(KeyCode key, bool isDown,  InputDelegate handler)       
        {
        	InputDelegate deleg;
        	var inputTable = isDown ? keyDownTable : keyUpTable;
    		if (inputTable.TryGetValue(key, out deleg)) 
    		{
    			deleg = Delegate.Remove(deleg, handler) as InputDelegate;
    			inputTable[key] = deleg;
    			if (inputTable[key] == null)
                {
                    inputTable.Remove(key);
                }
                bindCount--;
                //unbindString+= key.ToString();
    		}
    	}


    	public void AddAxisBind(int axis, AxisDelegate handler)
    	{
    		AxisDelegate deleg;
    		if (axisTable.TryGetValue(axis, out deleg))
          	{
          		axisTable[axis] = Delegate.Combine(deleg, handler) as AxisDelegate;
    			//UnityEngine.Debug.LogError(axis+" leaking invok list"+axisTable[axis].GetInvocationList().Length);
          	}
          	else
          	{
          		axisTable[axis] = handler;
          	}
    	}
    	public void RemoveAxisBind(int axis, AxisDelegate handler)       
        {
        	AxisDelegate deleg;
    		if (axisTable.TryGetValue(axis, out deleg)) 
    		{
    			deleg = Delegate.Remove(deleg, handler) as AxisDelegate;
    			axisTable[axis] = deleg;
    			if (axisTable[axis] == null)
                {
                    axisTable.Remove(axis);
                }
    		}
    	}


    	public void ConsumeFrame()
    	{
    		debugConsumedInputFrame = 3;
    	}

        public void ConsumeKeyDownLock()
        {
            consumeInputKeyDownLock = true;
        }

    	void OnGUI()
    	{
    		if (ConsumedInput)
    		{
    			if (debugConsumedInputFrame > 0)
    	        {
    	            debugConsumedInputFrame--;
    	        }

                if (consumeInputKeyDownLock && (Event.current == null || Event.current.type == EventType.KeyUp))
                    consumeInputKeyDownLock = false;
    			return;
    		}
    		if (Event.current != null && (Event.current.type == EventType.KeyUp || Event.current.type == EventType.KeyDown)) 
    		{
    			InputDelegate deleg;
    			var inputTable = (Event.current.type == EventType.KeyDown) ? keyDownTable : keyUpTable;
    			if (inputTable != null && inputTable.TryGetValue(Event.current.keyCode, out deleg))
    			{
    				hasShift = Event.current.shift;
    				hasCtrl = Event.current.control;
    				hasAlt = Event.current.alt;
    				hasCommand = Event.current.command;
    				deleg();
    			}
    		}
    	}

    	void Update()
    	{
    		int axisCount = axisNames.Length;
    		for(int i=0; i<axisCount; ++i)
    		{
    			String str = axisNames[i];
    			float delta = Input.GetAxis(str);
    			if (Mathf.Abs(delta) > 0)
    			{
    				AxisDelegate deleg;
    				if (axisTable.TryGetValue(i, out deleg))
    				{
    					deleg(delta);
    				}
    			}	
    		}
    	}
    }
}