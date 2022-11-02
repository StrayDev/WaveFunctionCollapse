using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Area
{
    // Members
    public string  Name     { get; private set; }
    public Vector3 Position { get; private set; }
    public Vector3 Bounds   { get; private set; }
    public Vector3 Extents  { get; private set; }
    
    public List<Chunk> Chunks { get; private set; }

    // Constructor
    public Area(string name)
    {
        Name = name;
        Chunks = new List<Chunk>();
        Chunks.Add(new Chunk(Position));
    }
    
    // Public
    public void CreateNewChunk(Chunk origin, Vector3 direction)
    {
        var position = origin.center + Vector3.Scale(direction, Chunk.Bounds);
        Chunks.Add(new Chunk(position));
    }
    
}
