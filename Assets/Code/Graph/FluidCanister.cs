using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidCanister : FluidPipeEdge {
    public FluidType initialFluid;
    public bool startFull = false;
    public MultiWaySwitch valve;

    private const float INITIAL_TEMP = 283f;
    
    protected new void Awake() {
        base.Awake();
        // Debug.Log($"{name}: FC.Awake()");
        FluidPipeData data = this.data as FluidPipeData;
        if (startFull) {
            container.count = 1;
            container.types[0] = initialFluid;
            container.temp[0] = INITIAL_TEMP;
            // PV = nRT
            container.mass[0] = (data.fullPressure * container.volume)
                / (Fluids.IDEAL_GAS_CONSTANT * container.temp[0]);
        }
        container.MarkDirty();
    }

    protected new void FixedUpdate() {
        base.FixedUpdate();
        if (valve.State > 0) {
            foreach (FluidPipeEdge edge in v1.edges) {
                // Debug.Log($"tick: {edge.name}");
                container.Equalize(edge.container);
            }
        }
    }
}
