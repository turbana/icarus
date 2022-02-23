using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

public class FluidPipeData : GraphData {
    public Material material;
    public float radius = 0.1f;
    public int sides = 6;
    public bool smooth = false;

    public override void GenerateObjects(GraphEdge edge) {
        // Debug.Log($"generate objects: {name}");
        RemoveChildObjects(edge);
        ProBuilderMesh mesh = GenerateMesh(edge);
        MoveMesh(edge, mesh);
    }

    private void RemoveChildObjects(GraphEdge edge) {
        while (edge.transform.childCount > 0) {
            Object.DestroyImmediate(edge.transform.GetChild(0).gameObject);
        }
    }

    private ProBuilderMesh GenerateMesh(GraphEdge edge) {
        float length = Vector3.Distance(edge.v1.transform.position, edge.v2.transform.position);
        ProBuilderMesh mesh = ShapeGenerator.GenerateCylinder(PivotLocation.Center, sides, radius, length, 0, smooth ? 1 : -1);
        GameObject go = mesh.transform.gameObject;
        go.AddComponent<BoxCollider>();
        go.GetComponent<BoxCollider>().center = Vector3.zero;
        mesh.GetComponent<Renderer>().sharedMaterial = material;
        return mesh;
    }

    private void MoveMesh(GraphEdge edge, ProBuilderMesh mesh) {
        mesh.transform.SetParent(edge.transform);
        Vector3 v1 = edge.v1.transform.position;
        Vector3 v2 = edge.v2.transform.position;
        Vector3 mid = Vector3.Lerp(v1, v2, 0.5f);
        edge.transform.position = mid;
        mesh.transform.localPosition = Vector3.zero;
        mesh.transform.rotation = Quaternion.FromToRotation(mesh.transform.up, mid - v1);
    }
}
