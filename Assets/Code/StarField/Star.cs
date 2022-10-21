using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour {
    public string title;
    public float ra;
    public float dec;
    public float mag;
    public float temp;
    
    public override string ToString() => $"<{title} ({ra}ra {dec}dec) {mag}mag {temp}K>";
}
