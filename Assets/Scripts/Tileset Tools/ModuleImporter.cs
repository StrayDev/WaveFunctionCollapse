using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ModuleImporter : MonoBehaviour
{
    public TileSetAsset so;

    public Dictionary<string, Vector3[]> sockets;

    public Dictionary<string, string> socketPairMap = new Dictionary<string, string>();
    private int _airHash;

    // face constants
        private const int Front = 0;
        private const int Back = 1;
        private const int Left = 2;
        private const int Right = 3;
        private const int Up   = Constants.Up;
        private const int Down = Constants.Down;

    /*    private const int Left  = Constants.Left;
        private const int Right = Constants.Right;
        private const int Front = Constants.Front;
        private const int Back  = Constants.Back;
        private const int Up    = Constants.Up;
        private const int Down  = Constants.Down;*/

    public void ImportModules()
    {
        sockets = new Dictionary<string, Vector3[]>();
        sockets.Clear();

        sockets.Add("-1", new Vector3[1]);

        socketPairMap.Clear();

        so.tileset.modules.Clear();

        AddAirModule(so.tileset);

        // get all of the mesh filters
        var meshFilterList = GetComponentsInChildren<MeshFilter>();
        var meshList = GetMeshList(meshFilterList);

        // register socket info and create prototype modules
        DefineSocketInformationFromMeshList(meshList, so.tileset);

        // create 3 rotations for each prototype
        DefineRotationVariants(so.tileset);

        // hack for error in import 
        // < < < This will need to be fixed
        for (var i = 0; i < so.tileset.modules.Count; i++)
        {
            if (so.tileset.modules[i].name.Contains("10"))
            {
                so.tileset.modules[i].sockets[Down] = "v2";
            }
        }

        // define possible neighbours
        DefinePossibleNeighbours(so.tileset);

        Debug.Log($"Modules Imported to : Scriptable Data - {so.tileset.assetName}");
    }

    private void DefineSocketInformationFromMeshList(Mesh[] meshList, TileSet tileset)
    {
        var uid = 0;

        foreach (var mesh in meshList)
        {
            var meshData = new MeshData(mesh);
            var module = new Module($"{uid++}__", 0, meshData);

            // for each face
            for (var i = 0; i < Constants.FaceCount; i++)
            {
                // determine if horizontal or vertical face
                var isVertical = i == Up || i == Down;

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
                    // check that m <--> f relation is correct
                    if (socketPairMap.ContainsKey(socket))
                    {
                        var isMale = socket.Contains('m');
                        var shouldBeMale = (i == Front || i == Left);

                        if ((!isMale && shouldBeMale || isMale && !shouldBeMale))
                        {
                            socket = socketPairMap[socket];
                        }
                    }

                    module.sockets.Add(socket);
                    continue;
                }

                if (module.sockets == null)
                {
                    Debug.Log("is Null");
                    return;
                }

                var newSocket = ProcessNewSocket(uid, isVertical, socketVertices);

                //
                if (socketPairMap.ContainsKey(newSocket))
                {
                    var isMale = socket.Contains('m');
                    var shouldBeMale = (i == Constants.Left || i == Constants.Front);

                    if ((!isMale && shouldBeMale || isMale && !shouldBeMale))
                    {
                        newSocket = socketPairMap[newSocket];
                    }
                }

                module.sockets.Add(newSocket);
            }

            // store the prototype 
            tileset.modules.Add(module);

        }
    }

    private void AddAirModule(TileSet tileset)
    {
        var module = new Module("Air", 0, null);
        for (int i = 0; i < 6; i++)
        {
            module.sockets.Add("-1");
        }
        tileset.modules.Add(module);
        _airHash = module.hash;
    }

    private void DefineRotationVariants(TileSet tileset)
    {
        var variants = new List<Module>();

        foreach (var original in tileset.modules)
        {
            // skip if all the side faces are the same
            if (AllSidesMatch(original)) continue;

            // copy the original mesh data
            var rotatedMeshData = new MeshData
            {
                vertices = original.meshData.vertices,
                normals = original.meshData.normals,
                triangles = original.meshData.triangles,
            };

            for (var i = 1; i < 4; i++)
            {
                var uid = ExtractUID(original.name);

                rotatedMeshData = RotateMeshData(rotatedMeshData);
                var variant = new Module($"{uid}__", i, rotatedMeshData);

                // set the sockets based on the rotation
                SetRotatedSockets(original, ref variant, i);

                variants.Add(variant);
            }
        }

        // add the rotation variants
        tileset.modules.AddRange(variants);
    }

    private MeshData RotateMeshData(MeshData meshData)
    {
        var m = meshData;

        var vertices = new SerializableVector3[meshData.vertices.Length];
        var normals = new SerializableVector3[meshData.normals.Length];

        for (var i = 0; i < meshData.vertices.Count(); i++)
        {
            vertices[i] = new SerializableVector3(m.vertices[i].z, m.vertices[i].y, -m.vertices[i].x);
            normals[i] = new SerializableVector3(m.normals[i].z, m.normals[i].y, -m.normals[i].x);
        }

        return new MeshData
        {
            vertices = vertices,
            normals = normals,
            triangles = meshData.triangles,
        };
    }

    private void SetRotatedSockets(Module original, ref Module variant, int rotation)
    {
        variant.sockets = new List<string>
        {
            "","","","","","",
        };

        // add top and bottom
        variant.sockets[Up] = original.sockets[Up];
        variant.sockets[Down] = original.sockets[Down];

        // rotate sides
        switch (rotation)
        {
            case 1:
                variant.sockets[Constants.Front] = original.sockets[Constants.Left];
                variant.sockets[Constants.Left]  = original.sockets[Constants.Back];
                variant.sockets[Constants.Back]  = original.sockets[Constants.Right];
                variant.sockets[Constants.Right] = original.sockets[Constants.Front];
                break;  
                
            case 2:                                                
                variant.sockets[Constants.Front] = original.sockets[Constants.Back];
                variant.sockets[Constants.Left]  = original.sockets[Constants.Right];
                variant.sockets[Constants.Back]  = original.sockets[Constants.Front];
                variant.sockets[Constants.Right] = original.sockets[Constants.Left];
                break;  
                
            case 3:                                                 
                variant.sockets[Constants.Front] = original.sockets[Constants.Right];
                variant.sockets[Constants.Left]  = original.sockets[Constants.Front];
                variant.sockets[Constants.Back]  = original.sockets[Constants.Left];
                variant.sockets[Constants.Right] = original.sockets[Constants.Back];
                break;

            default:
                Debug.LogError("SetRotatedSockets() should only recieve a value between 1 & 3");
                break;
        }
    }

    private string ExtractUID(string name)
    {
        var uid = string.Empty;
        foreach (char c in name)
        {
            if (char.IsDigit(c))
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
            for (var i = 0; i < Constants.FaceCount; i++)
            {
                // special case for air tile
                if (module.sockets[i] == "-1")
                {
                    module.neigbours[i].Add(_airHash);
                    continue;
                }

                // cycle through all of the other prototypes
                foreach (var other in tileset.modules)
                {
                    var otherFace = GetOtherFace(i);

                    // dont repeat neighbours
                    if (module.neigbours[i].Contains(other.hash)) continue;

                    // filter for m == f sockets
                    var isPaired = socketPairMap.ContainsKey(module.sockets[i]);
                    var isNotPairedToOther = true;

                    if (isPaired)
                    {
                        var value = socketPairMap[module.sockets[i]];
                        isNotPairedToOther = value != other.sockets[otherFace];

                        // Filter out non matching pairs
                        if (isNotPairedToOther) continue;

                        // on match add to list of neighbors
                        module.neigbours[i].Add(other.hash);
                        continue;
                    }

                    // filter out mismatches when not in m <-> f relationship
                    // skip if no match or
                    var doesNotMatch = !module.sockets[i].Equals(other.sockets[otherFace]);
                    if (doesNotMatch) continue;

                    // top and bottom rotation needs to match 
                    var isVertical = (i == Up || i == Down);
                    if (isVertical && module.rotation != other.rotation) continue;

                    // on match add to list of neighbors
                    module.neigbours[i].Add(other.hash);
                }
            }
        }
    }

    private int GetOtherFace(int i)
    {
        switch (i)
        {
            case Constants.Left : return Constants.Right;
            case Constants.Right: return Constants.Left;
            case Constants.Front: return Constants.Back;
            case Constants.Back : return Constants.Front;
            case Constants.Up   : return Constants.Down;
            case Constants.Down : return Constants.Up;
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

            // get rotated vertices if verticle
            var rotatedSocketVertices = new Vector3[socketVertices.Length];

            if (isVertical)
            {
                rotatedSocketVertices = GetVerticleRotatedVertices(socketVertices);
            }

            // compare verts
            var match = true;
            for (var i = 0; i < socketVertices.Length; i++)
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

    private Vector3[] GetVerticleRotatedVertices(Vector3[] sv)
    {
        var value = new Vector3[sv.Length];

        for (var i = 0; i < sv.Length; i++)
        {
            value[i] = new Vector3(-sv[i].z, sv[i].y, sv[i].x);
        }

        return value;
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

        socketPairMap.Add(socket + "m", socket + "f");
        socketPairMap.Add(socket + "f", socket + "m");

        sockets.Add(socket + "m", mirrorVertices);
        sockets.Add(socket += "f", socketVertices);
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

        // check if all vertices are inverted pars
        for (var j = 0; j < socketVertices.Count(); j++)
        {
            // check if vertices are inverted pars
            if (Math.Abs(socketVertices[j].x - -mirrorVertices[j].x) > .0001)
            {
                for (var v = 0; v < mirrorVertices.Length; v++)
                {
                    mirrorVertices[v].x *= -1;
                }
                return false;
            }
        }

        // check if all vertices are in a strait vertacle line
        var matchingCount = 0;
        for (var k = 0; k < socketVertices.Length; k++)
        {
            if (socketVertices[0].x != socketVertices[k].x) continue;
            matchingCount++;
        }

        if (matchingCount == socketVertices.Length) return false;

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
            case Constants.Left:
                foreach (var v in vertices) value.Add(new Vector3(-v.z, v.y, v.x));
                break;

            case Constants.Right:
                foreach (var v in vertices) value.Add(new Vector3(v.z, v.y, -v.x));
                break;

            case Constants.Front:
                foreach (var v in vertices) value.Add(new Vector3(-v.x, v.y, -v.z));
                break;

            case Constants.Back: // no transform needed
                return vertices;

            case Constants.Up:
                foreach (var v in vertices) value.Add(new Vector3(v.x, v.z, -v.y));
                break;

            case Constants.Down:
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
        Offset * Vector3.left,
        Offset * Vector3.right,
        Offset * Vector3.forward,
        Offset * Vector3.back,
        Offset * Vector3.up,
        Offset * Vector3.down,
    };

    private void OnDrawGizmos()
    {
        if (!so) return;
        if (so.tileset.modules.Count < 1) return;

        var renderers = GetComponentsInChildren<MeshRenderer>();

        if (so.tileset.modules.Count < renderers.Length) return;

        for (var r = 0; r < renderers.Length; r++)
        {
            var p = so.tileset.modules[r + 1];
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