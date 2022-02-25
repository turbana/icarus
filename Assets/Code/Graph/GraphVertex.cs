using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

public class GraphVertex : MonoBehaviour {
    public List<GraphEdge> edges;
    
    private const float GIZMO_RADIUS = 0.025f;

    void Start() {
        // Debug.LogFormat("{0} at {1}", name, transform.position);
    }

    public GraphEdge EdgeTo(GraphVertex other) {
        // Debug.Log($"{this.name}.EdgeTo({other.name})");
        foreach (GraphEdge edge in edges) {
            if ((edge.v1 == this && edge.v2 == other) ||
                (edge.v2 == this && edge.v1 == other)) {
                return edge;
            }
        }
        return null;
    }

    void OnDrawGizmos() {
        if (ShouldDrawGizmo() ) {
            if (GizmoIsSelected()) Gizmos.color = Color.green;
            else if (this.edges.Count > 3) Gizmos.color = Color.magenta;
            else if (this.edges.Count > 0) Gizmos.color = Color.blue;
            else Gizmos.color = Color.red;
            Gizmos.DrawSphere(this.transform.position, GIZMO_RADIUS);
        }
    }

    bool ShouldDrawGizmo() {
        foreach (GameObject o in Selection.gameObjects) {
            if (o.layer == this.gameObject.layer) {
                return true;
            }
        }
        return false;
    }

    bool GizmoIsSelected() {
        foreach (GameObject o in Selection.gameObjects) {
            if (o == gameObject) {
                return true;
            }
        }
        return false;
    }
}
