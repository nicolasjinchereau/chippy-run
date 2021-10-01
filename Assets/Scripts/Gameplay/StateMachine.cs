using System;
using UnityEngine;

public class StateMachine
{
	StateObject currentState;

	public StateMachine(StateObject firstState) {
		SetState(firstState);
	}

	public void SetState(StateObject next)
	{
        if(currentState) {
            currentState.Deactivate();
            currentState.stateMachine = null;
        }

        currentState = next;

        if(currentState) {
            currentState.stateMachine = this;
            currentState.Activate();
        }
	}

	public void Update() {
        currentState.Update();
	}

    public void OnPointerDown(UnityEngine.EventSystems.PointerEventData data)
    {
        if(currentState)
            currentState.OnPointerDown(data);
    }
}

public abstract class StateObject
{
	public StateMachine stateMachine = null;

	public void SetState(StateObject state) {
		stateMachine.SetState(state);
	}
	
	public static implicit operator bool(StateObject so) {
		return so != null;
	}
	
    public abstract void Activate();
    public abstract void Deactivate();
	public abstract void Update();

    public virtual void OnPointerDown(UnityEngine.EventSystems.PointerEventData data)
    {
        
    }
}
