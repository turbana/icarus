using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLEDFollower : BaseGameObject {
    public LEDSliderHandler slider;
    public MultiWaySwitch knob;
    
    void Start() {
        knob.AddListener(this);
    }

    protected override void OnChangeEvent(BaseGameObject _) {
        int state = (int)Math.Floor((double)slider.count * (knob.State + 1) / knob.count);
        slider.SetState(state);
    }
}
