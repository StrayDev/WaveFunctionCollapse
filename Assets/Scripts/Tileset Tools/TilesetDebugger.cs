using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilesetDebugger : MonoBehaviour
{
    [SerializeField] private TileSetAsset tileSetAsset;

    [SerializeField] private Material ModuleMat;
    [SerializeField] private Material NeighbourMat;

    private Dictionary<int, Module> ModuleHashMap;
    private Vector3 RollingPossition = Vector3.zero;
    private Vector3 Increment = new Vector3(3,0,0);
    private int Spaceing = 3;

    private List<Vector3> air_positions = new List<Vector3>();

    private void Start()
    {
        // get the dictionary of modules mapped
        ModuleHashMap = new Dictionary<int, Module>();
        foreach(var m in tileSetAsset.tileset.modules)
        {
            var hash = m.hash;
            if (!ModuleHashMap.ContainsKey(hash))
            {
                ModuleHashMap.Add(hash, m);
            }
        }


        foreach(var module in tileSetAsset.tileset.modules)
        {

            RollingPossition.x = 0;
            RollingPossition += Vector3.back * Spaceing;
            
            foreach(var neighbour_hash in module.neigbours.up)
            {
                RollingPossition.x += Spaceing;

                var neighbour = ModuleHashMap[neighbour_hash];
                CreateModuleNeighbourPair(module, neighbour, Vector3.up);
            }

            RollingPossition.x = 0;
            RollingPossition += Vector3.back * Spaceing;

            foreach (var neighbour_hash in module.neigbours.down)
            {
                RollingPossition.x += Spaceing;

                var neighbour = ModuleHashMap[neighbour_hash];
                CreateModuleNeighbourPair(module, neighbour, Vector3.down);
            }

            RollingPossition.x = 0;
            RollingPossition += Vector3.back * Spaceing;

            foreach (var neighbour_hash in module.neigbours.left)
            {
                RollingPossition.x += Spaceing;

                var neighbour = ModuleHashMap[neighbour_hash];
                CreateModuleNeighbourPair(module, neighbour, Vector3.left);
            }

            RollingPossition.x = 0;
            RollingPossition += Vector3.back * Spaceing;

            foreach (var neighbour_hash in module.neigbours.right)
            {
                RollingPossition.x += Spaceing;

                var neighbour = ModuleHashMap[neighbour_hash];
                CreateModuleNeighbourPair(module, neighbour, Vector3.right);
            }

            RollingPossition.x = 0;
            RollingPossition += Vector3.back * Spaceing;

            foreach (var neighbour_hash in module.neigbours.front)
            {
                RollingPossition.x += Spaceing;

                var neighbour = ModuleHashMap[neighbour_hash];
                CreateModuleNeighbourPair(module, neighbour, Vector3.forward);
            }

            RollingPossition.x = 0;
            RollingPossition += Vector3.back * Spaceing;

            foreach (var neighbour_hash in module.neigbours.back)
            {
                RollingPossition.x += Spaceing;

                var neighbour = ModuleHashMap[neighbour_hash];
                CreateModuleNeighbourPair(module, neighbour, Vector3.back);
            }
        }
    }

    private void CreateModuleNeighbourPair(Module module, Module neighbour, Vector3 offset)
    {
        CreateGameObjectFromModule(module, RollingPossition, ModuleMat);
        CreateGameObjectFromModule(neighbour, RollingPossition + offset, NeighbourMat);
    }

    private void CreateGameObjectFromModule(Module module, Vector3 position, Material material)
    {
        if (module.name.Contains("Air"))
        {
            air_positions.Add(position);
            return;
        }

        var obj = new GameObject(module.name);
        var fil = obj.AddComponent<MeshFilter>();
        var ren = obj.AddComponent<MeshRenderer>();

        obj.transform.position = position;

        fil.mesh = new Mesh();
        fil.mesh.vertices  = ConvertToVector3Array(module.meshData.vertices);
        fil.mesh.normals   = ConvertToVector3Array(module.meshData.normals);
        fil.mesh.triangles = module.meshData.triangles;

        ren.material = material;
        
    }

    private Vector3[] ConvertToVector3Array(SerializableVector3[] sv3Array)
    {
        var array = new Vector3[sv3Array.Length];
        for(var i = 0; i < sv3Array.Length; i++)
        {
            array[i] = new Vector3(sv3Array[i].x, sv3Array[i].y, sv3Array[i].z);
        }
        return array;
    }

    private void OnDrawGizmos()
    {
        foreach(var cell in air_positions)
        {
            Gizmos.DrawWireCube(cell, Vector3.one);
        }
    }
}
