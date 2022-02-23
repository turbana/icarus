using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        if (GUILayout.Button("Generate Geometry")) {
            foreach (GameObject go in Selection.gameObjects) {
                go.GetComponent<GraphEdge>().GenerateObjects();
            }
        }
    }

    [MenuItem("Tools/Generate All Graph Objects")]
    private static void GenerateAllObjects() {
        Debug.Log("generate all");
        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject root in roots) {
            foreach (GraphEdge edge in root.GetComponentsInChildren(typeof(GraphEdge))) {
                edge.GenerateObjects();
            }
        }
    }
}
