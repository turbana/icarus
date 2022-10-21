using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarField : MonoBehaviour {
    public float starDistance = 1000f;
    public Object catalog;
    public Sprite sprite;
    public Material material;
    public GameObject follow;

    public void Update() {
        this.transform.position = follow.transform.position;
    }
}
