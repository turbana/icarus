using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(LayoutCircleScript))]
public class ObjectBuilderEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        LayoutCircleScript script = (LayoutCircleScript)target;
        if (GUILayout.Button("Generate")) {
            script.Generate();
        }
    }
}
