using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoWaySwitch : MonoBehaviour, IInteractable {
    public GameObject mesh;
    public bool initialState = false;
    
    private bool state;
    
    void Start() {
        this.state = !this.initialState;
        SetState(this.initialState);
    }
    
    public void Interact() {
        SetState(!this.state);
    }

    public void ScrollUp() {
        SetState(true);
    }

    public void ScrollDown() {
        SetState(false);
    }

    public void SetState(bool wanted) {
        if (this.state == wanted) return;
        this.state = wanted;
        float rot = this.state ? -45f : 45f;
        this.mesh.transform.localRotation = Quaternion.Euler(rot, 0f, 0f);
        // Debug.LogFormat("switch is now: {0}", this.state);
    }

    public CrosshairType GetCrosshair() {
        return this.state ? CrosshairType.Down : CrosshairType.Up;
    }
}
