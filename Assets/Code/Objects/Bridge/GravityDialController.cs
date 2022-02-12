using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityDialController : BaseGameObject {
    public MultiWaySwitch dial;
    public GravitySource gravity;
    public float low = 0f;
    public float high = 0.88f;

    public float GravityForce {
        get {
            const float SHIP_RADIUS = 50f;
            Vector3 pos = gravity.transform.position + SHIP_RADIUS * Vector3.down;
            return gravity.ForceVector(pos).magnitude / 9.81f;
        }
    }
    
    void Start() {
        high = gravity.RotationRate * 2f;
        dial.AddListener(this);
    }

    protected override void OnChangeEvent(BaseGameObject _) {
        float percent = dial.State / (float)dial.count;
        gravity.RotationRate = high * percent;
        this.FireChangeEvent();
    }
}
