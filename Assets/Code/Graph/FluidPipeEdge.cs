using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidPipeEdge : GraphEdge {
    private int counter = 0;

    void FixedUpdate() {
        counter += 1;
        this.debug.text = $"#{counter}";
    }
}
