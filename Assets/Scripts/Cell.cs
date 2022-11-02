using System;
using UnityEngine;

[Serializable]
public class Cell
{
    // Static Constants
    public static readonly Vector3 Size;
    public static readonly Vector3 Extents;

    // Static Constructor
    static Cell()
    {
        Size = Vector3.one;
        Extents = Size / 2.0f;
    }
    
    // Constants
    public readonly Vector3 center;
    public readonly Vector3 zero; 
    
    // Constructor
    public Cell(Vector3 position)
    {
        center = position;
        zero = center - Extents;
    }


}
