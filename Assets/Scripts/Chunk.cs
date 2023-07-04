
// System
using System;
using System.Collections.Generic;

// Unity
using UnityEngine;

//

[Serializable]
public class Chunk
{
    // Static Constants
    public static readonly Vector3 Bounds = Vector3.one * 16;
    public static readonly Vector3 Extents = Bounds / 2;

    private static int Width  => (int)Bounds.x;
    private static int Height => (int)Bounds.y;
    private static int Depth  => (int)Bounds.z;
    
    public static readonly int CellCount = (int)(Bounds.x * Bounds.y * Bounds.z);
       
    // Constants
    public readonly Vector3 center;
    public readonly Vector3 zero;   

    public Cell[] cells;
    
    // Constructor
    public Chunk(Vector3 pos)
    {
        center = pos;
        zero = center - Extents;
        cells = PopulateChunk();
    }

    // Public Functions
    public WaveFunction GetWaveFunction(List<int> states)
    {
        return new WaveFunction(CellCount, states);
    }

    public Cell GetCell(Vector3 v) => GetCell((int)v.x, (int)v.y, (int)v.z);
    
    public Cell GetCell(int x, int y, int z)
    {
        return cells[XYZToIndex(x, y, z)];
    }
    
    public Cell GetCell(int index)
    {
        return cells[index];
    }

    private int[] NeighbourIndices = new int[6];

    public int[] GetCellNeighbourIndices(int index)
    {
        NeighbourIndices[0] = index - Width;
        NeighbourIndices[1] = index + Width;
        NeighbourIndices[2] = index - 1;
        NeighbourIndices[3] = index + 1;
        NeighbourIndices[4] = index + Width * Height;
        NeighbourIndices[5] = index - Width * Height;

        return NeighbourIndices;
    }

    // Private Functions
    private Cell[] PopulateChunk()
    {
        var array = new Cell[CellCount];

        for (var cell = 0; cell < CellCount; cell++)
        {
            IndexToXYZ(cell, out var x, out var y, out var z);
            array[cell] = new Cell(new Vector3(x, y, z));
        }

        return array;
    }
    
    // Source
    // https://stackoverflow.com/questions/7367770/how-to-flatten-or-index-3d-array-in-1d-array
    public int XYZToIndex(int x, int y, int z)
    {
        return (z * Width * Height) + (y * Width) + x;
    }

    public int Vec3ToIndex(Vector3 center)
    {
        return XYZToIndex((int)center.x, (int)center.y, (int)center.z);
    }

    // Source
    // https://stackoverflow.com/questions/7367770/how-to-flatten-or-index-3d-array-in-1d-array
    public void IndexToXYZ(int idx, out int x, out int y, out int z)
    {
        z = idx / (Width * Height);
        idx -= (z * Width * Height);
        y = idx / Width;
        x = idx % Width;
    }

    public static List<int> GetNeighboursByIndex(int index)
    {
        var value = new List<int>();

        int x = index % Width;
        int y = (index / Width) % Height;
        int z = index / (Width * Height);

        // Add neighbors, considering chunk boundaries
        if (x > 0) value.Add(index - 1); // Left neighbor
        else value.Add(-1);

        if (x < Width - 1) value.Add(index + 1); // Right neighbor
        else value.Add(-1);

        if (z < Depth - 1) value.Add(index + Width * Height); // Front neighbor
        else value.Add(-1);

        if (z > 0) value.Add(index - Width * Height); // Back neighbor
        else value.Add(-1);

        if (y < Height - 1) value.Add(index + Width); // Top neighbor
        else value.Add(-1);

        if (y > 0) value.Add(index - Width); // Bottom neighbor
        else value.Add(-1);

        return value;
    }

    internal static Vector3 GetCellPositionByIndex(int index)
    {
        var x = index % Width;
        var y = (index / Width) % Height;
        var z = index / (Width * Height);

        return new Vector3(x, y, z);
    }
}
