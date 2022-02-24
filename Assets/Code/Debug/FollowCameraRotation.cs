using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCameraRotation : MonoBehaviour {
    [HideInInspector]
    new private Camera camera;
    
    void Start() {
        camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
    }

    void Update() {
        this.transform.rotation = camera.transform.rotation;
    }
}
