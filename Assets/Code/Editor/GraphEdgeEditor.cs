using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;


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

    [MenuItem("Tools/Graph Objects - Generate All Meshes")]
    private static void GenerateAllMeshes() {
        Debug.Log("generate all");
        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject root in roots) {
            foreach (GraphEdge edge in root.GetComponentsInChildren(typeof(GraphEdge))) {
                edge.GenerateObjects();
            }
        }
    }

    [MenuItem("Tools/Graph Objects - Reset All Objects")]
    private static void ResetAllObjects() {
        if (EditorUtility.DisplayDialog(
                "Reset all graph script objects?",
                "This will DELETE and/or RESET all graph objects (fluid pipes, electrical grid, etc). Are you SURE?",
                "Yes", "Oh gawd, no"))
        {
            Debug.Log("Reseting all GraphVertexs...");
            foreach (GraphVertex vert in FindAll(typeof(GraphVertex))) {
                vert.edges.Clear();
            }
            Debug.Log("Deleting all GraphEdges...");
            foreach (GraphEdge edge in FindAll(typeof(GraphEdge))) {
                Object.DestroyImmediate(edge.gameObject);
            }
        }
        
    }

    // [MenuItem("Tools/Graph Objects - Fixup All Objects")]
    // private static void FixupAllObjects() {
    //     Debug.Log("Fixing all GraphVertexes...");
    //     foreach (GraphVertex vert in FindAll(typeof(GraphVertex))) {
    //         bool done = false;
    //         while (!done) {
    //             foreach (GraphEdge edge in vert.edges) {
    //                 Debug.Log($"found edge: {edge.name}");
    //             }
    //         }
    //     }
    // }

    private static Component[] FindAll(System.Type T) {
        List<Component> objects = new List<Component>();
        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject root in roots) {
            objects.AddRange(root.GetComponentsInChildren(T));
        }
        return objects.ToArray();
    }
}
