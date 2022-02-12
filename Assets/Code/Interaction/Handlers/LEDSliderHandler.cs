using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LEDSliderHandler : MultiWaySwitch {
    public MeshRenderer[] leds;
    public Material[] materials;
    public Material offMaterial;

    void Awake() {
        if (leds.Length != materials.Length || leds.Length != count) {
            throw new System.Exception("must define same number of states, leds, and materials");
        }
    }

    public override void SetState(int next) {
        base.SetState(next);
        for (int i=0; i<leds.Length; i++) {
            Material[] mats = leds[i].materials;
            mats[0] = (i < state) ? materials[i] : offMaterial;
            leds[i].materials = mats;
        }
    }
}
