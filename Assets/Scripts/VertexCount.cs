using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexCount : MonoBehaviour
{
    void Start()
    {
        var mesh = GetComponent<MeshFilter>().mesh;
        Debug.Log($"Verices : {mesh.vertices.Length}");
    }
}
