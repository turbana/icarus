using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

// NOTE: actual rotation is done in ExternalObject


public class RotatePlanet : MonoBehaviour {
    Volume vol;
    PhysicallyBasedSky sky;
    Vector3 initialSkyPos;
    
    void Start() {
        this.vol = this.GetComponent<Volume>();
        if (this.vol.sharedProfile) {
            if (this.vol.sharedProfile.TryGet<PhysicallyBasedSky>(out var skyObj)) {
                this.sky = skyObj;
                this.initialSkyPos = this.sky.planetCenterPosition.value;
            } else {
                Debug.LogError("no PhysicallyBasedSky found");
            }
        } else {
            Debug.LogError("no Volume.sharedProfile found");
        }
    }

    void Update() {
        this.sky.planetCenterPosition.value = this.transform.position;
    }

    void OnDestroy() {
        // restore the initial position, otherwise we risk modifying editor values
        if (this.initialSkyPos != null && this.sky != null) {
            this.sky.planetCenterPosition.value = this.initialSkyPos;
        }
    }
}
