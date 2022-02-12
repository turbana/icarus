using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChangeHandler : BaseGameObject {
    public MultiWaySwitch source;
    public int meshMaterialIndex;
    public Material[] materials;
    
    void Awake() {
        if (source != null) {
            source.AddListener(this);
        }
    }

    protected override void OnChangeEvent(BaseGameObject obj) {
        int state = ((MultiWaySwitch)obj).State;
        if (state < 0 || state >= this.materials.Length) {
            throw new System.Exception("state out of bounds");
        }
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        Material[] mats = renderer.materials;
        mats[meshMaterialIndex] = this.materials[state];
        renderer.materials = mats;
    }
}
