using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyGravity : MonoBehaviour {
    public GravitySource source;

    private Rigidbody rb;
    
    void Start() {
        this.rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate() {
        Vector3 force = this.source.ForceVector(this.transform.position);
        this.rb.AddForce(force * this.rb.mass);
    }
}
