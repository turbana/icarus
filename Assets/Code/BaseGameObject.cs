using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseGameObject : MonoBehaviour {
    private List<BaseGameObject> listeners = new List<BaseGameObject>();

    public void AddListener(BaseGameObject listener) {
        this.listeners.Add(listener);
    }

    protected virtual void OnChangeEvent(BaseGameObject obj) {
        throw new System.Exception("Not Implemented Error");
    }

    protected void FireChangeEvent() {
        foreach (BaseGameObject obj in this.listeners) {
            obj.OnChangeEvent(this);
        }
    }
}
