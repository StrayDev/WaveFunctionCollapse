
// System
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Unity
using UnityEngine;


//
public class ChunkTerrain : MonoBehaviour
{

    [SerializeField] private TileSetAsset tileSetAsset;

    private TileSet tileset;

    private Dictionary<int, Module> modules;

    [SerializeField] private Material material;

    void Start()
    {
        // cash the tileset
        tileset = tileSetAsset.tileset;

        modules = new Dictionary<int, Module>();

        foreach(var module in tileset.modules)
        {
            modules.Add(module.hash, module);
        }

        // eventually we will have this called on the correct chunk
        var chunk = new Chunk(Vector3.zero);
        
        WFCNewChunkGeneration(chunk, tileset);
    }

    private void WFCNewChunkGeneration(Chunk chunk, TileSet tileset)
    {
        // Create a list of possible states
        var allPossibleStates = GetModuleHashList();

        // Create the initial wave function based on the input
        var wave = chunk.GetWaveFunction(allPossibleStates);

        // create vars outside of loop to prevent stack overflow
        List<int> possibleStates;

        var index = 0;
        var state = 0;

        // Loop until wave function collapses
        while (!wave.IsCollapsed())
        {
            // get the cell and state info
            index = wave.SelectCellToCollapse();

            possibleStates = wave.GetPossibleStates(index);
            
            // currently just random
            state = SelectState(possibleStates);

            // Update the grid and wave function based on the selected state
            wave.CollapseCell(index, state);

            // Recursively update neighbouring cells possible states
            wave.UpdateNeighboursStateRecursive(index, chunk, modules);
        }

        Debug.Log("Generation Complete");

        Vector3[] vertices;
        Vector3[] normals;
        int[] triangles;

        for(var i = 0; i < chunk.cells.Length; i++)
        {
            if (wave.possibleStates[i] == null) continue;

            var hash = wave.possibleStates[i][0];
            var module = modules[hash];
            var meshdata = modules[hash].meshData;

            ConvertMeshData(meshdata, out vertices, out triangles, out normals);

            var go = new GameObject($"Cell {i}");

            go.transform.position = chunk.cells[i].center;

            var r = go.AddComponent<MeshRenderer>();

            r.material = material;

            var f = go.AddComponent<MeshFilter>();

            f.mesh = new Mesh();
            f.mesh.vertices = vertices;
            f.mesh.triangles = triangles;
            f.mesh.normals = normals;
            f.mesh.RecalculateNormals();

            var m = go.AddComponent<DebugModuleData>();
            m.SetModuleData(module);
        }
    }

    // this function needs to be moved or the conversion should be done at load time
    private void ConvertMeshData(MeshData meshdata, out Vector3[] vertices, out int[] triangles, out Vector3[] normals)
    {
        normals = new Vector3[meshdata.normals.Length];
        vertices = new Vector3[meshdata.vertices.Length];
        triangles = new int[meshdata.triangles.Length];

        for(int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(meshdata.vertices[i].x, meshdata.vertices[i].y, meshdata.vertices[i].z);
            normals[i] = new Vector3(meshdata.normals[i].x, meshdata.normals[i].y, meshdata.normals[i].z);
        }

        for (int i = 0; i < triangles.Length; i++)
        {
            triangles[i] = meshdata.triangles[i];
        }
    }

    private int SelectState(List<int> possibleStates)
    {
        var i = UnityEngine.Random.Range(0, possibleStates.Count - 1);
        return possibleStates[i];
    }

    private List<int> GetModuleHashList()
    {
        var hashList = new List<int>();

        foreach (var m in tileset.modules)
        {
            hashList.Add(m.hash);
        }

        return hashList;
    }

    private void GenerateChunkTerrain(TileSet tileset, Vector3 chunkPos)
    {
        // create grid
        var chunk = new Chunk(chunkPos);

        // create our dictionary of modules
        var modules = new Dictionary<int, Module>();

        foreach (var module in tileset.modules)
        {
            modules.Add(module.hash, module);
        }

        // add all hash to every cell
        var possibleCellModules = new List<int>(chunk.cells.Length);
        var cellEntropy = new List<int>(chunk.cells.Length);

        foreach (var cell in chunk.cells)
        {
            possibleCellModules.AddRange(modules.Keys);
            cellEntropy.Add(modules.Count);
        }

        // stash values
        var isZeroEnropy = false;
        var maxEntropy = modules.Count - 1;
        var totalEntropy = (modules.Count - 1) * chunk.cells.Length;

        var resolvedCellIndices = new List<int>();

        // loop until resolution
        while (!isZeroEnropy)
        {
            var cellIndex = GetLowestEntropyCell(maxEntropy, possibleCellModules);

            CollapseCell(ref totalEntropy, cellIndex, cellEntropy, possibleCellModules);

            // Add cell to list of resolved cells
            resolvedCellIndices.Add(cellIndex);

            // Update neigbours
            UpdateCellNeighbours(chunk, cellIndex, resolvedCellIndices);

            if (true/*totalEntropy == 0*/) break;
        }

    }

    private void UpdateCellNeighbours(Chunk chunk, int cellIndex, List<int> resolvedCells)
    {
        var cellsToResolve = new List<int>();

        // add the first cell starts the cascade
        cellsToResolve.Add(cellIndex);

        var tempResolvedCells = new List<int>();
        var tempResolvedCellCount = resolvedCells.Count;

        while (tempResolvedCellCount < chunk.cells.Length)
        {
            foreach (var index in cellsToResolve)
            {
                var cell = chunk.GetCell(cellIndex);
                var cellPosition = cell.center;

                // get indices for neigbors
                var neighbours = new List<int>
                {
                    chunk.Vec3ToIndex(cellPosition + Vector3.up),
                    chunk.Vec3ToIndex(cellPosition + Vector3.down),
                    chunk.Vec3ToIndex(cellPosition + Vector3.left),
                    chunk.Vec3ToIndex(cellPosition + Vector3.right),
                    chunk.Vec3ToIndex(cellPosition + Vector3.forward),
                    chunk.Vec3ToIndex(cellPosition + Vector3.back),
                };

                foreach (var neighbour in neighbours)
                {
                    // dont re-add resolved neighbours
                    if (tempResolvedCells.Contains(neighbour)) continue;

                    //var cellCollapsed = UpdatePossibleModules();

                    tempResolvedCells.Add(neighbour);

                }
            }
        }

    }

    private void CollapseCell(ref int totalEntropy, int cellIndex, List<int> cellEntropy, List<int> modules)
    {
        var randomIndex = UnityEngine.Random.Range(0, modules.Count - 1);

        // set entropy to zero
        totalEntropy -= cellEntropy[cellIndex];
        cellEntropy[cellIndex] = 0;

        // clear the list keeping only the selected module
        var selectedModule = modules[randomIndex];

        modules.Clear();
        modules.Add(selectedModule);

    }

    private int GetLowestEntropyCell(int maxEntropy, List<int> cellEntropys)
    {
        var lowestEntropy = maxEntropy;
        var cellIndex = -1;

        for (var i = 0; i < cellEntropys.Count; i++)
        {
            var entropy = cellEntropys[i];

            if (entropy < 1) continue;

            if (entropy < lowestEntropy)
            {
                lowestEntropy = cellEntropys[i];
                cellIndex = i;
            }
        }

        if (lowestEntropy == maxEntropy) return cellEntropys.Count / 2; // << should be 0,0,0 will need to be changed

        return cellIndex;
    }
}
