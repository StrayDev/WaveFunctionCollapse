
// System
using System;
using System.Collections.Generic;

// Unity
using UnityEngine;

//
[Serializable]
public class TileSet
{
    public string assetName = "Default TileSet";
    public List<Module> modules = new List<Module>();
}

[System.Serializable]
public class Module
{
    public Module() { } 

    public Module(string name, int rotation, MeshData meshData)
    {
        this.name = name + " R" + rotation;
        this.rotation = rotation;
        hash = this.name.GetHashCode();

        this.meshData = meshData;
    }

    public string name = string.Empty;
    public int hash = 0;
    public int rotation = 0;

    public MeshData meshData;

    // each socket/face is labled with a hash id 
    public List<string> sockets = new List<string>(6);

    // module contains 6 lists of valid neighbours
    public Neigbours neigbours = new Neigbours();
}

[System.Serializable]
public class Neigbours
{
    public List<int> left  = new List<int>();
    public List<int> right = new List<int>();
    public List<int> front = new List<int>();
    public List<int> back  = new List<int>();
    public List<int> up    = new List<int>();
    public List<int> down  = new List<int>();

    public List<int> this[int index]
    {
        get
        {
            switch (index)
            {
                case Constants.Left:  return left;
                case Constants.Right: return right;
                case Constants.Front: return front;
                case Constants.Back:  return back;
                case Constants.Up:    return up;
                case Constants.Down:  return down;
                default:
                    throw new IndexOutOfRangeException("Invalid index: " + index);
            }
        }
    }
}

[System.Serializable]
public class MeshData
{
    public MeshData() {} // Required

    public MeshData(Mesh mesh)
    {
        triangles = mesh.triangles;

        // we cant serialize Vector3 so we have to convert it 
        vertices = new SerializableVector3[mesh.vertices.Length];

        for(int i = 0; i < mesh.vertices.Length; i++)
        {
            vertices[i] = new SerializableVector3(mesh.vertices[i]);
        }

        normals = new SerializableVector3[mesh.normals.Length];

        for (int i = 0; i < mesh.normals.Length; i++)
        {
            normals[i] = new SerializableVector3(mesh.normals[i]);
        }
    }

    public int[] triangles;
    public SerializableVector3[] normals;
    public SerializableVector3[] vertices;
}

[System.Serializable]
public class SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(Vector3 vector3)
    {
        x = vector3.x;
        y = vector3.y;
        z = vector3.z;
    }

    public SerializableVector3(float _x, float _y, float _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}


