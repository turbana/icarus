using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphData : MonoBehaviour {
    public virtual void GenerateObjects(GraphEdge edge) {}
    public virtual void Tick(GraphEdge edge) {}
}
