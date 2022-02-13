using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LayoutCircleScript : MonoBehaviour {
    public GameObject prefab;
    public float radius;
    public int count;
    
    public void Generate() {
        Undo.SetCurrentGroupName("Generate Circle From Prefabs");
        float degreeStep = 360f / this.count;

        for (int n=0; n<this.count; n++) {
            CreatePrefab(n * degreeStep);
        }
    }

    GameObject CreatePrefab(float degrees) {
        GameObject obj = PrefabUtility.InstantiatePrefab(this.prefab) as GameObject;
        obj.transform.SetParent(this.transform, false);
        obj.transform.localPosition = Vector3.down * this.radius;
        obj.transform.rotation = Quaternion.Euler(Vector3.zero);
        obj.transform.RotateAround(this.transform.position, Vector3.forward, degrees);
        StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(this.gameObject);
        GameObjectUtility.SetStaticEditorFlags(obj, flags);
        Undo.RegisterCreatedObjectUndo(obj, "Created Prefab Instance");
        return obj;
    }
}
