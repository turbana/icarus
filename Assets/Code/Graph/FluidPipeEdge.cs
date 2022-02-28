using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidPipeEdge : GraphEdge {
    [HideInInspector]
    public FluidContainer container;
    
    protected new void Awake() {
        base.Awake();
        // Debug.Log($"{name}: FPE.Awake()");
        FluidPipeData data = this.data as FluidPipeData;
        float height = Vector3.Distance(this.v1.transform.position,
                                        this.v2.transform.position);
        // assume half of pipe is usable space
        float radius = data.radius / 2f;
        float volume = Mathf.PI * Mathf.Pow(radius, 2f) * height;
        container = this.gameObject.AddComponent<FluidContainer>();
        // 1000L per m3
        container.volume = volume * 1000f;
        container.fullPressure = data.fullPressure;
        // AddRandomElements();
        container.MarkDirty();
    }

    void AddRandomElements() {
        int fluids = System.Enum.GetNames(typeof(FluidType)).Length;
        container.count = Random.Range(0, 3);
        for (int i=0; i<container.count; i++) {
            container.types[i] = (FluidType)Random.Range(0, fluids);
            container.mass[i] = Random.Range(0.01f, 1f);
            container.temp[i] = Random.Range(273f, 300f);
        }
    }

    protected void FixedUpdate() {
        string text = "";
        for (int i=0; i<container.count; i++) {
            text += $"{container.types[i].ShortName(),3}: {container.mass[i]}mol ({container.temp[i]:F2}K) @{container.pressure[i]}atm\n";
        }
        // text += $"Prs: {container.fullPressure}atm\n";
        // text += $"Avl: {container.massAvailable}mol\n";
        text += $"Vol: {container.volume}L @{container.totalPressure}atm";
        this.debug.text = text;
    }
}
