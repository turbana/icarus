using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExternalObject : MonoBehaviour {
    public GravitySource source;
    
    void Start() {
        
    }

    void Update() {
        float rate = this.source.RotationRate * Mathf.Rad2Deg;
        this.transform.RotateAround(this.source.transform.position,
                                    this.source.RotationAxis,
                                    rate * Time.deltaTime);
    }
}
