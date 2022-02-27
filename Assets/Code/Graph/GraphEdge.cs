using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GraphEdge : MonoBehaviour {
    public GraphVertex v1;
    public GraphVertex v2;
    public GraphData data;
    
    protected TextMeshPro debug;

    protected void Awake() {
        // Debug.Log($"{name}: {(v1 == null ? "null" : v1.name)} -> {(v2 == null ? "null" : v2.name)}");
        GameObject go = DebugText.CreateTextObject(this.transform);
        debug = go.GetComponent<TextMeshPro>();
        debug.text = this.name;
    }

    public void GenerateObjects() {
        this.data.GenerateObjects(this);
    }
}
