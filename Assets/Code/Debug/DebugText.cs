using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DebugText : MonoBehaviour {
    public static GameObject CreateTextObject(Transform parent) {
        GameObject go = GameObject.Find("DebugTextPrototype");
        if (go != null) {
            go = Object.Instantiate(go, parent);
            go.transform.localPosition = Vector3.zero;
            go.name = "DebugText";
        }
        return go;
    }
}
