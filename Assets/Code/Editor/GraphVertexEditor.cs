using UnityEditor;
using UnityEngine;
using System.Collections;


[CustomEditor(typeof(GraphVertex))]
[CanEditMultipleObjects]
public class GraphVertexEditor : Editor {
    private string[] edgeClassChoices = new[] {"FluidGraphEdge"};
    private int edgeClassChoice = 0;

    public override void OnInspectorGUI() {
        // base.OnInspectorGUI();
        if (Selection.objects.Length == 1) {
            DrawDefaultInspector();
        } else if (Selection.count == 2) {
            GraphVertex v1 = Selection.gameObjects[0].GetComponent<GraphVertex>();
            GraphVertex v2 = Selection.gameObjects[1].GetComponent<GraphVertex>();
            GraphEdge edge = v1.EdgeTo(v2);
            if (edge != null) {
                if (GUILayout.Button("Remove Connection")) {
                    Debug.Log($"Removing connection between {v1.gameObject.name} and {v2.gameObject.name}");
                    Undo.IncrementCurrentGroup();
                    Undo.SetCurrentGroupName("Remove graph object connection");
                    Undo.RecordObject(v1, "Remove graph edge from vertex");
                    Undo.RecordObject(v2, "Remove graph edge from vertex");
                    v1.RemoveEdge(edge);
                    v2.RemoveEdge(edge);
                    EditorUtility.SetDirty(v1);
                    EditorUtility.SetDirty(v2);
                    Undo.DestroyObjectImmediate(edge.gameObject);
                }
            } else {
                edgeClassChoice = EditorGUILayout.Popup(edgeClassChoice, edgeClassChoices);
                if (GUILayout.Button("Create Connection")) {
                    string layer = LayerMask.LayerToName(v1.gameObject.layer);
                    GameObject parent = GameObject.Find(layer);
                    if (parent == null) {
                        Debug.LogError($"Could not find object {layer}");
                        return;
                    }
                    Debug.Log($"Creating connection between {v1.gameObject.name} and {v2.gameObject.name}");
                    Undo.IncrementCurrentGroup();
                    Undo.SetCurrentGroupName("Add graph object connection");
                    GameObject go = new GameObject();
                    go.transform.parent = parent.transform;
                    go.layer = v1.gameObject.layer;
                    go.name = GlobalObjectId.GetGlobalObjectIdSlow(go).ToString();
                    if (edgeClassChoice == 0) {
                        edge = go.AddComponent<FluidPipeEdge>() as GraphEdge;
                    }
                    edge.v1 = v1;
                    edge.v2 = v2;
                    Undo.RegisterCreatedObjectUndo(go, "Created graph edge");
                    Undo.RecordObject(v1, "Adding graph edge to vertex");
                    Undo.RecordObject(v2, "Adding graph edge to vertex");
                    v1.AddEdge(edge);
                    v2.AddEdge(edge);
                    EditorUtility.SetDirty(v1);
                    EditorUtility.SetDirty(v2);
                }
            }
        } else {
            GUILayout.Label("Cannot connect more than two GraphVertexs.");
        }
    }
}
