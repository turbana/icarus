using UnityEditor;
using UnityEngine;
using System.Collections;


[CustomEditor(typeof(GraphVertex))]
[CanEditMultipleObjects]
public class GraphVertexEditor : Editor {
    private const float GIZMO_SIZE = 0.025f;

    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
    static void DrawGizmosSelected(GraphVertex vert, GizmoType gizmoType) {
        if (ShouldDrawGizmo(vert)) {
            bool selected = (gizmoType & GizmoType.Selected) == GizmoType.Selected;
            Gizmos.color = selected ? Color.blue : Color.red;
            Gizmos.DrawSphere(vert.transform.position, GIZMO_SIZE);
        }
    }

    static bool ShouldDrawGizmo(GraphVertex vert) {
        foreach (GameObject o in Selection.gameObjects) {
            if (o.layer == vert.gameObject.layer) {
                return true;
            }
        }
        return false;
    }

    public override void OnInspectorGUI() {
        if (Selection.objects.Length == 1) {
            DrawDefaultInspector();
        } else if (Selection.count == 2) {
            GraphVertex v1 = Selection.gameObjects[0].GetComponent<GraphVertex>();
            GraphVertex v2 = Selection.gameObjects[1].GetComponent<GraphVertex>();
            GraphEdge edge = v1.EdgeTo(v2);
            if (edge != null) {
                if (GUILayout.Button("Remove Connection")) {
                    Debug.Log($"Removing connection between {v1.gameObject.name} and {v2.gameObject.name}");
                    v1.edges.Remove(edge);
                    v2.edges.Remove(edge);
                    Object.DestroyImmediate(edge.gameObject);
                }
            } else {
                if (GUILayout.Button("Create Connection")) {
                    string layer = LayerMask.LayerToName(v1.gameObject.layer);
                    GameObject parent = GameObject.Find(layer);
                    if (parent == null) {
                        Debug.LogError($"Could not find object {layer}");
                        return;
                    }
                    Debug.Log($"Creating connection between {v1.gameObject.name} and {v2.gameObject.name}");
                    GameObject go = new GameObject();
                    go.transform.parent = parent.transform;
                    go.layer = v1.gameObject.layer;
                    go.name = GlobalObjectId.GetGlobalObjectIdSlow(go).ToString();
                    edge = go.AddComponent<GraphEdge>() as GraphEdge;
                    edge.v1 = v1;
                    edge.v2 = v2;
                    v1.edges.Add(edge);
                    v2.edges.Add(edge);
                }
            }
        } else {
            GUILayout.Label("Cannot connect more than two GraphVertexs.");
        }
    }
    
    // protected virtual void OnSceneGUI() {
    //     GraphVertex vert = target as GraphVertex;
    //     if (Event.current.type == EventType.Repaint || true) {
    //         Handles.color = Color.grey;
    //         Handles.SphereHandleCap(
    //             0,
    //             vert.transform.position,
    //             vert.transform.rotation,
    //             GIZMO_SIZE,
    //             EventType.Repaint
    //         );
    //     }
    // }
}
