using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiWaySwitch : BaseGameObject {
    public int count = 2;
    public int initialState = 0;

    protected int state;
    public int State { get => state; }

    public virtual void Start() {
        SetState(initialState);
        StartCoroutine(StartLate());
    }

    IEnumerator StartLate() {
        yield return new WaitForEndOfFrame();
        FireChangeEvent();
        yield return null;
    }

    public bool CanIncrementState() {
        return state < (count - 1);
    }

    public bool CanDecrementState() {
        return state > 0;
    }

    public void IncrementState() {
        if (CanIncrementState()) {
            SetState(state + 1);
        }
    }

    public void DecrementState() {
        if (CanDecrementState()) {
            SetState(state - 1);
        }
    }

    public virtual void SetState(int next) {
        this.state = next;
        FireChangeEvent();
    }
}
