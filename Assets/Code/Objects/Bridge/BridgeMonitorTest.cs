using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BridgeMonitorTest : BaseGameObject {
    public MultiWaySwitch[] switches;
    public GravityDialController gravDial;

    private TextMeshPro text;
    
    void Start() {
        this.text = GetComponentInChildren<TextMeshPro>();
        foreach (MultiWaySwitch sw in this.switches) {
            sw.AddListener(this);
        }
        gravDial.AddListener(this);
    }

    string OnOff(MultiWaySwitch sw) {
        return (sw.State == 0) ? "OFF" : " ON";
    }

    float Percent(MultiWaySwitch sw) {
        return 100f * sw.State / sw.count;
    }

    protected override void OnChangeEvent(BaseGameObject _) {
        float rot = gravDial.gravity.RotationRate;
        float gs = gravDial.GravityForce;
        this.text.SetText(
            $@"           BRIDGE MONITOR

  LIGHTS   = {OnOff(switches[0])}
  ROT SPD  = {rot,4:F2}/rs  G={gs,4:F2}

  TEST SW1 = {OnOff(switches[2])}    TEST SW2 = {OnOff(switches[3])}
  TEST SW3 = {OnOff(switches[4])}    TEST SW4 = {OnOff(switches[5])}
"
        );
    }
}
