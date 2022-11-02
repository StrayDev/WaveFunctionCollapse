using System;
using UnityEngine;

[Serializable]
public class Chunk
{
    // Static Constants
    public static readonly Vector3 Bounds;
    public static readonly Vector3 Extents;

    private static readonly int Width;
    private static readonly int Height;
    private static readonly int Depth;
    
    private static readonly int CellCount;
    
    // Static Constructor
    static Chunk()
    {
        Bounds      = new Vector3(16, 16, 16);
        Extents   = Bounds / 2;
        Width     = (int)Bounds.x;
        Height    = (int)Bounds.y;
        Depth     = (int)Bounds.z;
        CellCount = (int)(Bounds.x * Bounds.y * Bounds.z);  
    }
    
    // Constants
    public Cell[] cells; // should be readonly
    
    public readonly Vector3 center;
    public readonly Vector3 zero;   
    
    // Constructor
    public Chunk(Vector3 pos)
    {
        center = pos;
        zero = center - Extents;
        
        cells = PopulateChunk();
    }
    
    // Public Functions
    public Cell GetCell(Vector3 v) => GetCell((int)v.x, (int)v.y, (int)v.z);
    
    public Cell GetCell(int x, int y, int z)
    {
        return cells[XYZToIndex(x, y, z)];
    }
    
    public Cell GetCell(int index)
    {
        return cells[index];
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
    private int XYZToIndex(int x, int y, int z)
    {
        return (z * Width * Height) + (y * Width) + x;
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
}
