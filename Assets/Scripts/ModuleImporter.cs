using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ModuleImporter : MonoBehaviour
{
    public int socketCount = 0;

    public Dictionary<string, Vector3[]> sockets;
    public List<Prototype> prototypes;

    [ContextMenu("Align to grid")]
    public void AlignToGrid()
    {
        var width = 16;
        var offset = 2;
        var x = 0;
        var z = 0;

        foreach (Transform obj in transform)
        {
            if (x >= width)
            {
                x = 0;
                z += offset;
            }

            x += offset;
            obj.position = new Vector3(x, 0, z);
        }
    }

    [ContextMenu("Import Modules")]
    public void ImportModules()
    {
        //debug = new List<Vector2Int>();
        sockets = new Dictionary<string, Vector3[]>();
        prototypes = new List<Prototype>();

        // get all of the mesh filters
        var meshFilterList = GetComponentsInChildren<MeshFilter>();
        var meshList = GetMeshList(meshFilterList);

        // keep track of uid for each socket
        var uid = 0;

        // register socket info -------------
        foreach (var mesh in meshList)
        {
            var prototype = new Prototype("__", mesh, 0);

            for (var i = 0; i < 6; i++)
            {
                // get socket vertices aligned to the x axis
                var rotatedVertices = GetRotatedVertices(i, mesh.vertices);
                var socketVertices = GetSocketVertices(rotatedVertices);

                
                // set invalid for invalid / empty
                if (socketVertices == null)
                {
                    prototype.sockets[i] = "-1";
                    continue;
                }
                
                // check if socket already exists
                if (SocketExists(socketVertices, out var socket))
                {
                    prototype.sockets[i] = socket;
                    continue;
                }

                prototype.sockets[i] = ProcessNewSocket(uid, socketVertices);
                uid++;
            }

            // store the prototype -------------
            prototypes.Add(prototype);
        }

        socketCount = sockets.Count;

        // define possible neighbours
    }

    private bool SocketExists(Vector3[] socketVertices, out string socket)
    {
        socket = string.Empty;
        
        foreach (var kvp in sockets)
        {
            if (kvp.Value.Length != socketVertices.Length) continue;

            var v1 = GetHashFromArray(kvp.Value);
            var v2 = GetHashFromArray(socketVertices);

            if (v1 != v2) continue;
            
            socket = kvp.Key;
            return true;
        }

        return false;
    }

    private int GetHashFromArray(Vector3[] array)
    {
        var hash = 0;
        foreach (var obj in array)
        {
            hash += obj.GetHashCode();
        }
        return hash;
    }

    private string ProcessNewSocket(int uid, Vector3[] socketVertices)
    {
        var socket = uid.ToString();

        // check for symmetry
        if (IsSymmetrical(socketVertices, out var mirrorVertices))
        {
            sockets.Add(socket += 's', socketVertices);
            return socket;
        }

        sockets.Add(socket, socketVertices);
        sockets.Add(socket += 'f', mirrorVertices);
        return socket;
    }

    private static bool IsSymmetrical(Vector3[] socketVertices, out Vector3[] mirrorVertices)
    {
        mirrorVertices = socketVertices;

        // filp the vertices on the x axis
        for (var i = 0; i < mirrorVertices.Length; i++)
        {
            mirrorVertices[i].x *= -1;
        }

        return socketVertices == mirrorVertices;
    }

    private Vector3[] GetSocketVertices(Vector3[] vertices)
    {
        //return vertices.Where(v => Math.Abs(v.z - (-.5f)) < .01f).ToArray();

        // if linq no worky
        var value = new List<Vector3>();
        foreach (var v in vertices)
        {
            if (Math.Abs(v.z - (-.5f)) < .001f)
            {
                value.Add(v);
            }
        }

        if (value.Count == 0) return null;
        
        value.Reverse();
        return value.ToArray();
    }

    private Vector3[] GetRotatedVertices(int face, Vector3[] vertices)
    {
        // init out array
        var value = new List<Vector3>(vertices.Length);

        switch (face)
        {
            case 0: // x +
                foreach (var v in vertices) value.Add(new Vector3(v.z, v.y, -v.x));
                break;

            case 1: // x -
                foreach (var v in vertices) value.Add(new Vector3(-v.z, v.y, v.x));
                break;

            case 2: // y +
                foreach (var v in vertices) value.Add(new Vector3(v.x, v.z, -v.y));
                break;

            case 3: // y -
                foreach (var v in vertices) value.Add(new Vector3(v.x, -v.z, v.y));
                break;

            case 4: // z +
                foreach (var v in vertices) value.Add(new Vector3(v.x, v.y, -v.z));
                break;

            case 5: // z -
                return vertices;
                
        }

        return value.ToArray();
    }

    private Mesh[] GetMeshList(MeshFilter[] filters)
    {
        var meshes = new Mesh[filters.Length];
        for (var i = 0; i < filters.Length; i++)
        {
            meshes[i] = filters[i].sharedMesh;
        }

        return meshes;
    }

    // Source
    // https://www.cuemath.com/questions/how-to-rotate-a-figure-90-degrees-clockwise-about-a-point/
    private Vector3[] RotateVertices90Degrees(Vector3[] vertices)
    {
        for (var i = 0; i < vertices.Length; i++)
        {
            var v = vertices[i];
            vertices[i] = new Vector3(v.z, v.y, -v.x);
        }

        return vertices;
    }
    
    // Gizmos
    private const float Offset = 0.7f;

    private Vector3[] _offsetList =
    {
        Offset * Vector3.right,
        Offset * Vector3.left,
        Offset * Vector3.up,
        Offset * Vector3.down,
        Offset * Vector3.forward,
        Offset * Vector3.back
    };
    
    private void OnDrawGizmos()
    {
        var renderers = GetComponentsInChildren<MeshRenderer>();

        for (var r = 0; r < renderers.Length; r++)
        {
            var p = prototypes[r];
            var t = renderers[r].transform;
            
            Handles.DrawWireCube(t.position, Vector3.one);
            
            for (var i = 0; i < 6; i++)
            {
                Handles.Label(t.position + _offsetList[i], p.sockets[i]);
            }
        }
    }
}