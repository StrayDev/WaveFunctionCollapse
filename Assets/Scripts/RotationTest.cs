using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationTest : MonoBehaviour
{
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private bool DoThing = false;



    private void Update()
    {
        if(DoThing)
        {
            var mesh = meshFilter.mesh;
            var vertices = mesh.vertices;
            var normals = mesh.normals;

            for(int i = 0; i < mesh.vertices.Length; i++)
            {
                vertices[i] = new Vector3(vertices[i].z, vertices[i].y, -vertices[i].x);
                normals[i] = new Vector3(normals[i].z, normals[i].y, -normals[i].x);
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            DoThing = false;
        }
    }
}
