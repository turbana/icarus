using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleSwitchInteract : MonoBehaviour, IInteractable {
    public ToggleSwitch toggle;

    public void Interact() {
        if (toggle.State == 0) {
            toggle.IncrementState();
        } else {
            toggle.DecrementState();
        }
    }

    public void ScrollUp() {
        toggle.IncrementState();
    }

    public void ScrollDown() {
        toggle.DecrementState();
    }

    public CrosshairType GetCrosshair() {
        return toggle.State == 0 ? CrosshairType.Up : CrosshairType.Down;
    }
}
