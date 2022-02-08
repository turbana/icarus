using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnobInteractLeft : MonoBehaviour, IInteractable {
    public KnobSwitch knob;

    public void Interact() {
        knob.DecrementState();
    }

    public void ScrollUp() {
        knob.IncrementState();
    }

    public void ScrollDown() {
        knob.DecrementState();
    }

    public CrosshairType GetCrosshair() {
        return CrosshairType.CCW;
    }
}
