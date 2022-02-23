using UnityEditor;
using UnityEngine;
using System.Collections;


[CustomEditor(typeof(GraphEdge))]
[CanEditMultipleObjects]
public class GraphEdgeEditor : Editor {
    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
    static void DrawGizmosSelected(GraphEdge edge, GizmoType gizmoType) {
        if (ShouldDrawGizmo(edge)) {
            bool selected = (gizmoType & GizmoType.Selected) == GizmoType.Selected;
            bool hasData = edge.data != null;
            Gizmos.color = selected ? Color.green : (hasData ? Color.blue : Color.red);
            Gizmos.DrawLine(edge.v1.transform.position, edge.v2.transform.position);
        }
    }

    static bool ShouldDrawGizmo(GraphEdge edge) {
        foreach (GameObject o in Selection.gameObjects) {
            if (o.layer == edge.gameObject.layer) {
                return true;
            }
        }
        return false;
    }
}
