using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitchHandler : BaseGameObject {
    public ToggleSwitch source;
    public GameObject[] lights;
    
    void Start() {
        source.AddListener(this);
    }

    protected override void OnChangeEvent(BaseGameObject obj) {
        bool enabled = this.source.State != 0;
        foreach (GameObject light in this.lights) {
            light.SetActive(enabled);
        }
    }
}
