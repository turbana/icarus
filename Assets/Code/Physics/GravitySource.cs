using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravitySource : MonoBehaviour {
    public float RotationSpeed = 0.0f;
    public float Radius = 50.0f;
    
    public Vector3 ForceVector(Vector3 location) {
        Vector3 origin = new Vector3(this.transform.position.x,
                                     this.transform.position.y,
                                     location.z);
        Vector3 down = -(origin - location).normalized;
        // TODO change force to account for distance / velocity
        float force = 9.81f;
        return down * force;
    }
}
