using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidPipeData : GraphData {
    public Material material;
    public float radius = 0.1f;
    public int sides = 6;
    public bool smooth = false;

    public override void GenerateObjects(GraphEdge edge) {
        Debug.Log($"generate objects: {name}");
    }
}
