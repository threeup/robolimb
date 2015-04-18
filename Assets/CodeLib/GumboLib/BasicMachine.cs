using System.Collections.Generic;
using UnityEngine;

namespace GumboLib
{
	//General
	[System.Serializable]
	public class BasicState 
	{
		public object enumValue = null;
		public string enumName = "UNDEFINED";
		
		public int idx;
		public delegate void OnChangeDelegate();
		public delegate bool CanEnterDelegate();

		public OnChangeDelegate OnEnter;
		public OnChangeDelegate OnExit;
		public CanEnterDelegate CanEnter;

		public BasicState(int i, object eVal, string eName)
		{
			idx = i;
			OnEnter = null;
			OnExit = null;
			CanEnter = DefaultEnter;
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

	public class BasicMachine<T> where T : struct
	{

		public delegate void OnChangeDelegate(int idx);
		public OnChangeDelegate OnChange;

		public BasicState currentState;
	    public BasicState previousState;
		public BasicState failedState;
		public List<BasicState> stateList;
		public bool isInitialized = false;
		protected System.Type enumType;

		public virtual void Initialize(System.Type eType)
		{
			enumType = eType;
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

		public void ClearPreviousState()
		{
			previousState = stateList[0];
		}

	    public int GetPreviousState()
	    {
	        return previousState.idx;
	    }

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
			return currentState.enumValue.Equals(tValue);
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
				
				if (previousState.OnExit != null) { previousState.OnExit(); }
				if (OnChange != null) { OnChange(previousState.idx); }
				if (nextState.OnEnter != null) { nextState.OnEnter(); }
				return true;
			}
			failedState = nextState;
			return false;
		}

		public bool? SetLastState()
		{
			return SetState(previousState);
		}

		public void RepeatEnter()
		{
			if (currentState.OnExit != null) { currentState.OnExit(); }
			if (currentState.OnEnter != null) { currentState.OnEnter(); }
		}

		public override string ToString()
		{
			return currentState.enumName;
		}
	}
}