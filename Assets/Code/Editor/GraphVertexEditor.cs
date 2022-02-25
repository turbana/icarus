using UnityEditor;
using UnityEngine;
using System.Collections;


[CustomEditor(typeof(GraphVertex))]
[CanEditMultipleObjects]
public class GraphVertexEditor : Editor {
    private string[] edgeClassChoices = new[] {"FluidGraphEdge"};
    private int edgeClassChoice = 0;

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
                edgeClassChoice = EditorGUILayout.Popup(edgeClassChoice, edgeClassChoices);
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
                    if (edgeClassChoice == 0) {
                        edge = go.AddComponent<FluidPipeEdge>() as GraphEdge;
                    }
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
}
