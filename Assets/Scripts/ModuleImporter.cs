using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class ModuleImporter : MonoBehaviour
{
    public TileSetAsset so;

    public Dictionary<string, Vector3[]> sockets;

    // face constants
    private const int Front = 0;
    private const int Back = 1;
    private const int Left = 2;
    private const int Right = 3;
    private const int Top = 4; 
    private const int Bottom = 5; 
    
    private const int NumFaces = 6;
    
    public void ImportModules()
    {
        sockets = new Dictionary<string, Vector3[]>();
        sockets.Add("-1", new Vector3[1]);
        
        // get all of the mesh filters
        var meshFilterList = GetComponentsInChildren<MeshFilter>();
        var meshList = GetMeshList(meshFilterList);

        // register socket info and create prototype modules
        DefineSocketInformationFromMeshList(meshList, so.tileset);

        // create 3 rotations for each prototype
        DefineRotationVariants(so.tileset);

        // define possible neighbours
        DefinePossibleNeighbours(so.tileset);

        Debug.Log($"Modules Imported to {so.tileset.assetName}");
    }

    private void DefineSocketInformationFromMeshList(Mesh[] meshList, TileSet tileset)
    {
        var uid = 0;

        foreach (var mesh in meshList)
        {
            var meshData = new MeshData(mesh);
            var module = new Module($"{uid++}__", 0, meshData);

            // for each face
            for (var i = 0; i < NumFaces; i++)
            {
                // determine if horizontal or vertical face
                var isVertical = i == Top || i == Bottom;

                // get socket vertices aligned to the x axis
                var rotatedVertices = GetRotatedVertices(i, mesh.vertices);
                var socketVertices = GetSocketVertices(rotatedVertices);

                // set invalid for invalid / empty
                if (socketVertices == null)
                {
                    module.sockets.Add("-1");
                    continue;
                }

                // check if socket already exists
                if (SocketExists(socketVertices, isVertical, out var socket))
                {
                    module.sockets.Add(socket);
                    continue;
                }

                if (module.sockets == null)
                {
                    Debug.Log("is Null");
                    return;
                }

                var newSocket = ProcessNewSocket(uid, isVertical, socketVertices);
                module.sockets.Add(newSocket); 
            }

            // store the prototype 
            tileset.modules.Add(module);

        }
    }

    private void DefineRotationVariants(TileSet tileset)
    {
        var variants = new List<Module>();
        foreach (var original in tileset.modules)
        {
            // skip if all the side faces are the same
            if (AllSidesMatch(original)) continue;

            for (var i = 1; i < 4; i++)
            {
                var uid = ExtractUID(original.name);

                var variant = new Module($"{uid}__", i, original.meshData);

                // set the sockets based on the rotation
                SetRotatedSockets(original, ref variant, i);

                variants.Add(variant);
            }
        }

        // add the rotation variants
        tileset.modules.AddRange(variants);
    }

    private void SetRotatedSockets(Module original, ref Module variant, int rotation)
    {
        variant.sockets = new List<string>
        {
            "","","","","","",
        };

        // add top and bottom
        variant.sockets[Top] = original.sockets[Top];
        variant.sockets[Bottom] = original.sockets[Bottom];

        // rotate sides
        switch(rotation)
        {
            case 1:
                variant.sockets[Front] = original.sockets[Left];
                variant.sockets[Left] = original.sockets[Back];
                variant.sockets[Back] = original.sockets[Right];
                variant.sockets[Right] = original.sockets[Front];
                break;
            case 2:
                variant.sockets[Front] = original.sockets[Back];
                variant.sockets[Left] = original.sockets[Right];
                variant.sockets[Back] = original.sockets[Front];
                variant.sockets[Right] = original.sockets[Left];
                break;
            case 3:
                variant.sockets[Front] = original.sockets[Right];
                variant.sockets[Left] = original.sockets[Front];
                variant.sockets[Back] = original.sockets[Left];
                variant.sockets[Right] = original.sockets[Back];
                break;
            default:
                Debug.LogError("SetRotatedSockets() should only recieve a value between 1 & 3");
                break;
        }
    }

    private string ExtractUID(string name)
    {
        var uid = string.Empty;
        foreach(char c in name)
        {
            if(char.IsDigit(c))
            {
                uid += c;
                continue;
            }
            return uid;
        }
        Debug.LogError("Unable to extract UID from name");
        return string.Empty;
    }

    private bool AllSidesMatch(Module module)
    {
        var socket0 = module.sockets[0];
        var matchinFaces = 0;

        // rotations 2 - 4
        for (var s = 1; s < 4; s++) 
        {
            if (socket0 != module.sockets[s]) break;
            matchinFaces++;
        }

        // no need for variant if all sides are the same
        return matchinFaces > 2;
    }

    private void DefinePossibleNeighbours(TileSet tileset)
    {
        foreach (var module in tileset.modules)
        {

            // we check for each face
            for (var i = 0; i < 6; i++)
            {
                // cycle through all of the other prototypes
                foreach (var other in tileset.modules)
                {
                    var otherFace = GetOtherFace(i);

                    // dont repeat neighbours
                    if (module.neigbours[i].Contains(other.hash)) continue;

                    // skip if no match 
                    if (!module.sockets[i].Equals(other.sockets[otherFace])) continue;

                    // on match add to list of neighbors
                    module.neigbours[i].Add(other.hash);
                }
            }
        }
    }

    private int GetOtherFace(int i)
    {
        switch(i)
        {
            case 0: return 1;
            case 1: return 0;
            case 2: return 3;
            case 3: return 2;
            case 4: return 5;
            case 5: return 4;
            default: 
                Debug.LogError("GetOtherFace() should only take a value between 0 & 5");
                return -1;
        }
    }

    private bool SocketExists(Vector3[] socketVertices, bool isVertical, out string socket)
    {
        socket = string.Empty;

        foreach (var kvp in sockets)
        {
            // check if number of verts is the same
            if (kvp.Value.Length != socketVertices.Length) continue;

            // separates out the vertical and side faces
            var keyContainsV = kvp.Key.Contains('v'); 
            if (keyContainsV && !isVertical || !keyContainsV && isVertical) continue;

            // compare verts
            var match = true;
            for(var i = 0; i < socketVertices.Length; i++)
            {
                if (kvp.Value.Contains(socketVertices[i])) continue;
                match = false;
                break;
            }
            
            // all vertices are the same
            if (match)
            {
                socket = kvp.Key;
                return true;
            }
        }

        // no match found
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

    private string ProcessNewSocket(int uid, bool isVertical, Vector3[] socketVertices)
    {
        var socket = uid.ToString();
        
        // vertical sockets only care about rotation
        if (isVertical)
        {
            sockets.Add(socket += "v", socketVertices);
            return socket;
        }
        
        // check for symmetry
        if (IsSymmetrical(socketVertices, out var mirrorVertices))
        {
            sockets.Add(socket += "s", socketVertices);
            return socket;
        }

        sockets.Add(socket + "f", mirrorVertices);
        sockets.Add(socket += "m", socketVertices);
        return socket;
    }

    private static bool IsSymmetrical(Vector3[] socketVertices, out Vector3[] mirrorVertices)
    {
        var count = socketVertices.Length;
        mirrorVertices = new Vector3[count];

        // copy in reverse order
        for (var i = 0; i < count; i++)
        {
            mirrorVertices[i] = socketVertices[count - 1 - i];
        }

        // check if al vertices are inverted pars
        for(var j = 0; j < socketVertices.Count(); j++)
        {
            // check if vertices are inverted pars
            if (Math.Abs(socketVertices[j].x - -mirrorVertices[j].x) > .0001)
            {
                for(var v = 0; v < mirrorVertices.Length; v++)
                {
                    mirrorVertices[v].x *= -1;
                }
                return false;
            }
        }
        return true;
    }

    private Vector3[] GetSocketVertices(Vector3[] vertices)
    {
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
            case Back: // no transform needed
                return vertices;

            case Front: 
                foreach (var v in vertices) value.Add(new Vector3(-v.x, v.y, -v.z));
                break;

            case Right: 
                foreach (var v in vertices) value.Add(new Vector3(v.z, v.y, -v.x));
                break;

            case Left:
                foreach (var v in vertices) value.Add(new Vector3(-v.z, v.y, v.x));
                break;

            case Top: 
                foreach (var v in vertices) value.Add(new Vector3(v.x, v.z, -v.y));
                break;

            case Bottom: 
                // Note - dropped the inversion -v.z for matching with topface
                foreach (var v in vertices) value.Add(new Vector3(v.x, v.z, v.y));
                break;                           
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
    private Vector3[] RotateVertices90Degrees(ref Vector3[] vertices)
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
        Offset * Vector3.forward,
        Offset * Vector3.back,
        Offset * Vector3.left,
        Offset * Vector3.right,
        Offset * Vector3.up,
        Offset * Vector3.down,
    };
    
    private void OnDrawGizmos()
    {
        if(!so) return;
        if (so.tileset.modules.Count < 1) return;
        
        var renderers = GetComponentsInChildren<MeshRenderer>();
        
        if (so.tileset.modules.Count < renderers.Length) return;
            
        for (var r = 0; r < renderers.Length; r++)
        {
            var p = so.tileset.modules[r];
            var t = renderers[r].transform;

            Handles.color = Color.white;
            Handles.DrawWireCube(t.position, Vector3.one);
            
            for (var i = 0; i < 6; i++)
            {
                Handles.Label(t.position + _offsetList[i], p.sockets[i]);
            }
        }
    }
}