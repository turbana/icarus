using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

public class GraphVertex : MonoBehaviour {
    public GraphEdge[] edges;
    
    private const float GIZMO_RADIUS = 0.025f;

    void Start() {
        // Debug.LogFormat("{0} at {1}", name, transform.position);
    }

    public void AddEdge(GraphEdge edge) {
        GraphEdge[] next = new GraphEdge[edges.Length + 1];
        edges.CopyTo(next, 0);
        next[edges.Length] = edge;
        edges = next;
    }

    public void RemoveEdge(GraphEdge edge) {
        GraphEdge[] next = new GraphEdge[edges.Length - 1];
        int j = 0;
        for (int i=0; i<edges.Length; i++) {
            if (edges[i] != edge) {
                next[j++] = edges[i];
            }
        }
        edges = next;
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
            else if (this.edges.Length > 3) Gizmos.color = Color.magenta;
            else if (this.edges.Length > 0) Gizmos.color = Color.blue;
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

    protected void FixedUpdate() {
        if (edges.Length > 1) {
            for (int i = 0; i < edges.Length - 1; i++) {
                TryEqualize(edges[i], edges[i + 1]);
            }
            if (edges.Length > 2) {
                TryEqualize(edges[0], edges[edges.Length - 1]);
            }
        }
    }

    private void TryEqualize(GraphEdge e1, GraphEdge e2) {
        FluidPipeEdge fpe1 = e1 as FluidPipeEdge;
        FluidPipeEdge fpe2 = e2 as FluidPipeEdge;
        if (fpe1 != null && fpe2 != null) {
            fpe1.container.Equalize(fpe2.container);
        }
    }
}
