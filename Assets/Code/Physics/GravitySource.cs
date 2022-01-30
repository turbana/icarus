using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravitySource : MonoBehaviour {
    public float RotationRate = 0.44f; // in radians
    public Vector3 RotationAxis = Vector3.forward;
    
    public Vector3 ForceVector(Vector3 location) {
        Vector3 offset = this.PositionVector(location);
        Vector3 down = -offset.normalized;
        // centripetal force: a = rw^2
        float force = offset.magnitude * this.RotationRate * this.RotationRate;
        return down * force;
    }

    public Vector3 PositionVector(Vector3 location) {
        Vector3 origin = new Vector3(this.transform.position.x,
                                     this.transform.position.y,
                                     location.z);
        return origin - location;
    }
}
