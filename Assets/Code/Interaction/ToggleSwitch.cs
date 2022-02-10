using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleSwitch : MultiWaySwitch {
    public float degrees = 45f;

    protected override void SetState(int next) {
        base.SetState(next);
        float rot = degrees * (state == 0 ? 1 : -1);
        this.transform.localRotation = Quaternion.Euler(rot, 0f, 0f);
    }
}
