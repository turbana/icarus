using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnobInteractRight : MonoBehaviour, IInteractable {
    public KnobSwitch knob;

    public void Interact() {
        knob.IncrementState();
    }

    public void ScrollUp() {
        knob.IncrementState();
    }

    public void ScrollDown() {
        knob.DecrementState();
    }

    public CrosshairType GetCrosshair() {
        return CrosshairType.CW;
    }
}
