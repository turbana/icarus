using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphEdge : MonoBehaviour {
    public GraphVertex v1;
    public GraphVertex v2;
    public GraphData data;

    void Start() {
        Debug.Log($"{name}: {(v1 == null ? "null" : v1.name)} -> {(v2 == null ? "null" : v2.name)}");
    }
}
