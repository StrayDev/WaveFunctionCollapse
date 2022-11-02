using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Sockets
{
    public int pX;
    public int nX;
    public int pY;
    public int nY;
    public int pZ;
    public int nZ;
}

public struct Neighbors
{
    public List<int> pX;
    public List<int> nX;
    public List<int> pY;    
    public List<int> nY;    
    public List<int> pZ;  
    public List<int> nZ;
}

[System.Serializable]
public class Module
{
    // each socket/face is labled with a hash id 
    public int[] sockets = new int[6];
    
    // module contains 6 lists of valid neighbours
    public List<int>[] neighbors = new List<int>[6]
    {
        new List<int>(),
        new List<int>(),
        new List<int>(),
        new List<int>(),
        new List<int>(),
        new List<int>(),
    };
}

[System.Serializable]
public class Prototype
{
    public string name;
    public int hashId;
    
    public Mesh mesh;
    public int rotation;

    // each socket/face is labled with a hash id 
    public string[] sockets = new string[6];
    
    // module contains 6 lists of valid neighbours
    public List<int>[] neighbors = new List<int>[6]
    {
        new List<int>(),
        new List<int>(),
        new List<int>(),
        new List<int>(),
        new List<int>(),
        new List<int>(),
    };
    
    public Prototype(string name, Mesh mesh, int rotation)
    {
        this.name = name + " R" + rotation;
        this.mesh = mesh;
        this.rotation = rotation;
        
        this.hashId = this.name.GetHashCode();
    }
}
