using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnobSwitch : MultiWaySwitch {
    public float leftStop = 270f;
    public float rightStop = 90f;

    private float step;

    public override void Start() {
        this.step = ((360f - leftStop) + rightStop) / this.count;
        base.Start();
    }

    protected override void SetState(int next) {
        base.SetState(next);
        float degrees = (leftStop + (this.state * step)) % 360f;
        this.transform.localRotation = Quaternion.Euler(0f, degrees, 0f);
    }
}
