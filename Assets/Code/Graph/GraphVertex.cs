using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class GraphVertex : MonoBehaviour {
    [HideInInspector] new public Transform transform;
    public List<GraphEdge> edges;

    void Reset() {
        this.transform = GetComponent<Transform>();
    }
    
    void Start() {
        Debug.LogFormat("{0} at {1}", name, transform.position);
    }

    public GraphEdge EdgeTo(GraphVertex other) {
        foreach (GraphEdge edge in edges) {
            if ((edge.v1 == this && edge.v2 == other) ||
                (edge.v2 == this && edge.v1 == other)) {
                return edge;
            }
        }
        return null;
    }
}
