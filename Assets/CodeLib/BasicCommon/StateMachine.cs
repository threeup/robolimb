using System.Collections.Generic;
using System;


public enum GeneralState
{
    NOTREADY,
    READY,
    ACTIVE,
    BUSY,
}

[System.Serializable]
public class GeneralMachine : StateMachine<GeneralState>
{
}

//General
[System.Serializable]
public class BasicState 
{
	public object enumValue = null;
	public string enumName = "UNDEFINED";
	
	public int idx;
	public delegate void OnStateDelegate(object owner);
	public delegate bool CanEnterDelegate();
	public delegate void UpdateDelegate(float deltaTime);

	public OnStateDelegate OnEnter;
	public OnStateDelegate OnExit;
	public CanEnterDelegate CanEnter;
	public UpdateDelegate DoUpdate;

	public BasicState(int i, object eVal, string eName)
	{
		idx = i;
		OnEnter = null;
		OnExit = null;
		CanEnter = DefaultEnter;
		DoUpdate = null;
		enumValue = eVal;
		enumName = eName;
	}

	public void Noop()
	{
		return;
	}

	public bool DefaultEnter()
	{
		return true;
	}
}
	
[System.Serializable]
public class StateMachine<T> where T : struct
{

	public delegate void OnChangeDelegate(int idx);
	public OnChangeDelegate OnChange;
	
	public BasicState currentState;
    public BasicState previousState;
	public BasicState failedState;
	public List<BasicState> stateList;
	public bool isInitialized = false;
	protected System.Type enumType;
	public Object owner;

	public virtual void Initialize(Object own)
	{
		owner = own;
		enumType = typeof(T);
		System.Array eVals = System.Enum.GetValues(enumType);
		int count = eVals.Length;
		stateList = new List<BasicState>(count);
		
		for (int i=0; i < count; ++i)
		{
			object enumValue = eVals.GetValue(i);
			stateList.Add(new BasicState(i, enumValue, System.Enum.GetName(enumType, enumValue)));
		}
        previousState = stateList[0];
		currentState = stateList[0];
		failedState = null;

		isInitialized = true;
	}

	public BasicState this[int i]
	{
		get { return stateList[i]; }
		set {}
	}

    public int GetPreviousState()
    {
        return previousState.idx;
    }

    public T State { get { return (T)(object)GetActiveState(); } }
	public int GetActiveState()
	{
		return currentState.idx;
	}
	public BasicState GetStateByType(T type)
	{
		return stateList[(int)(System.ValueType)type];
	}

	public bool IsState(T tValue)
	{
		return isInitialized && currentState.enumValue.Equals(tValue);
	}

	public bool? SetState(T type, bool forced = false)
	{
		return SetState(GetStateByType(type), forced);
	}

	public bool? SetState(BasicState nextState, bool forced = false)
	{
		if (nextState == currentState)
		{
			return null;
		}

		if (forced || nextState.CanEnter())
		{
			failedState = null;
            previousState = currentState; 
			currentState = nextState;
			
			if (previousState.OnExit != null) { previousState.OnExit(owner); }
			if (OnChange != null) { OnChange(previousState.idx); }
			if (nextState.OnEnter != null) { nextState.OnEnter(owner); }
			return true;
		}
		failedState = nextState;
		return false;
	}

	public bool? SetLastState()
	{
		return SetState(previousState);
	}

	public override string ToString()
	{
		return currentState.enumName;
	}

	public virtual void MachineUpdate(float deltaTime)
	{
		if (currentState.DoUpdate != null)
		{
			currentState.DoUpdate(deltaTime);
		}
	}

	public void AddEnterListener(BasicState.OnStateDelegate deleg)
	{
		string input = deleg.Method.Name;
		input = input.Replace("On","");
		T stateType = (T)System.Enum.Parse(typeof(T), input, true);
		
		int state = GetStateByType(stateType).idx;
		if (stateList[state].OnEnter == null)
		{
			stateList[state].OnEnter = deleg;
		}
		else
		{
			stateList[state].OnEnter += deleg;
		}
	}

	public void AddExitListener(BasicState.OnStateDelegate deleg)
	{
		string input = deleg.Method.Name;
		input = input.Replace("Off","");
		T stateType = (T)System.Enum.Parse(typeof(T), input, true);
		
		int state = GetStateByType(stateType).idx;
		if (stateList[state].OnExit == null)
		{
			stateList[state].OnExit = deleg;
		}
		else
		{
			stateList[state].OnExit += deleg;
		}
	}


	public void AddUpdateListener(BasicState.UpdateDelegate deleg)
	{
		string input = deleg.Method.Name;
		input = input.Replace("Update","");
		T stateType = (T)System.Enum.Parse(typeof(T), input, true);
		
		int state = GetStateByType(stateType).idx;
		if (stateList[state].DoUpdate == null)
		{
			stateList[state].DoUpdate = deleg;
		}
		else
		{
			stateList[state].DoUpdate += deleg;
		}
	}

	public void AddChangeListener(OnChangeDelegate deleg)
	{
		if (OnChange == null)
		{
			OnChange = deleg;
		}
		else
		{
			OnChange += deleg;
		}
	}

}