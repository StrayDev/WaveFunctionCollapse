
// System
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;

// Unity
using UnityEngine;

//
public class TerrainGeneration : MonoBehaviour
{

    [SerializeField] private TileSetAsset tileSetAsset;
    [SerializeField] private Material mat;

    private List<Module> Modules => tileSetAsset.tileset.modules;

    private Wave wave;

    private async void Start()
    {
        await GenerateTerrainAsync();

        Debug.Log("Terrain Generation Complete");
    }

    private async Task GenerateTerrainAsync()
    {
        // Unobserved state has a list of all modules
        var unobserved_state = Modules;

        // Create wave containing superpositions for every cell
        wave = new Wave(Chunk.CellCount, unobserved_state);

        
/*        var index = Chunk.CellCount / 4;
        CollapseSuperPosition(wave, wave.Superpositions[index]);
        wave.LastCollapsedCell = index;
        Propagation(wave);*/


        // Repeat the next steps
        while (true)
        {
            // When observation fails Generation should be complete
            if (!Observation(wave)) break;

            await Task.Delay(1);

            // Propagate changes in state
            Propagation(wave);

            await Task.Delay(1);

        }

        /*        var index = 0;
                foreach (var cell in wave.Superpositions)
                {
                    if (cell.GetEntropy() != 1) continue;

                    var go = new GameObject($"{index}");
                    var position = Chunk.GetCellPositionByIndex(index++);

                    go.transform.position = position;

                    // set the mesh
                    var meshdata = cell.Singularity.meshData;
                    var f = go.AddComponent<MeshFilter>();
                    f.mesh = new Mesh
                    {
                        // these need to be in order
                        vertices = ToVector3Array(meshdata.vertices),
                        normals = ToVector3Array(meshdata.normals),
                        triangles = meshdata.triangles,
                    };
                    f.mesh.RecalculateNormals();

                    var r = go.AddComponent<MeshRenderer>();
                    r.material = mat;
                }*/
    }

    private bool Observation(Wave wave)
    {
        // Find superposition with lowest entropy if you cant find one break the loop
        if (!TryGetMinimalNonSingularEntropy(wave, out var super_position))
        {
            return false;
        }

        // Collapse the superposition to a single option based on possible states
        CollapseSuperPosition(wave, super_position);
        return true;
    }

    private bool TryGetMinimalNonSingularEntropy(Wave wave, out Superposition super_position)
    {
        super_position = null;

        var lowest_value = int.MaxValue;
        var cell_index = -1;

        for (var cell = 0; cell < wave.Superpositions.Count; cell++)
        {
            var cell_entropy = wave.Superpositions[cell].GetEntropy();

            // filter out collapsed superpositions
            if (cell_entropy == 1) continue;

            if (cell_entropy < lowest_value)
            {
                // record the lowest entropy and the coresponding superposition
                lowest_value = cell_entropy;
                super_position = wave.Superpositions[cell];
                cell_index = cell;
            }
        }

        if (lowest_value == int.MaxValue)
        {
            // failed to observe, break the loop
            super_position = null;
            return false;
        }

        wave.LastCollapsedCell = cell_index;
        return true;
    }

    private void CollapseSuperPosition(Wave wave, Superposition super_position)
    {
        // not sure this is the correct approach
        var entropy = super_position.GetEntropy();
        var index = Random.Range(0, entropy);
        var singularity = super_position.States[index];

        super_position.States.Clear();
        super_position.States.Add(singularity);

        // < < < DEBUG < < <
        CreateDebugMesh(wave, singularity);
    }

    private void CreateDebugMesh(Wave wave, Module singularity)
    {
        //
        var go = new GameObject($"{singularity.name}");
        var position = Chunk.GetCellPositionByIndex(wave.LastCollapsedCell);

        go.transform.position = position;

        // set the mesh
        var meshdata = singularity.meshData;
        var f = go.AddComponent<MeshFilter>();
        f.mesh = new Mesh
        {
            // these need to be in order
            vertices = ToVector3Array(meshdata.vertices),
            normals = ToVector3Array(meshdata.normals),
            triangles = meshdata.triangles,
        };
        f.mesh.RecalculateNormals();

        var r = go.AddComponent<MeshRenderer>();
        r.material = mat;
    }

    private Vector3[] ToVector3Array(SerializableVector3[] array)
    {
        var value = new Vector3[array.Length];
        for (var i = 0; i < array.Length; i++)
        {
            value[i] = new Vector3(array[i].x, array[i].y, array[i].z);
        }
        return value;
    }

    private void Propagation(Wave wave)
    {
        // add the last collapsed cell to a que
        var handled = new List<int>();
        var queue = new Queue<int>();
        var end_queue = new Queue<int>();

        queue.Enqueue(wave.LastCollapsedCell);

        // while que not empty
        while (queue.Count > 0)
        {
            var index = queue.Dequeue();
            var neighbour_indices = Chunk.GetNeighboursByIndex(index);
            var singularity = wave.Superpositions[index].Singularity;

            // foreach neighbour 
            for (var side = 0; side < Constants.FaceCount; side++)
            {
                // cashe the index
                var n_index = neighbour_indices[side];
                
                wave.LastCollapsedCell = n_index;

                // -1 represents a cell outside of the chunk
                if (n_index == -1) continue;

                // skip if already updated
                if (handled.Contains(n_index)) continue;

                var neighbour = wave.Superpositions[n_index];

                UpdateNeighbour(wave, singularity, neighbour, side);

                // add neighbour to que
                if (queue.Contains(n_index) || end_queue.Contains(n_index)) continue;

                end_queue.Enqueue(n_index);
            }

            // make sure the queue updates in order
            while (end_queue.Count > 0) queue.Enqueue(end_queue.Dequeue());

            // add cell to handled list 
            handled.Add(index);
        }
    }

    private void UpdateNeighbour(Wave wave, Module singularity, Superposition neighbour, int side)
    {
        // return for collapsed neighbours
        if (neighbour.GetEntropy() == 1) return;

        switch (side)
        {
            case Constants.Left:
                RemoveImpossibleStates(wave, singularity.neigbours.left, neighbour);
                break;

            case Constants.Right:
                RemoveImpossibleStates(wave, singularity.neigbours.right, neighbour);
                break;

            case Constants.Front:
                RemoveImpossibleStates(wave, singularity.neigbours.front, neighbour);
                break;

            case Constants.Back:
                RemoveImpossibleStates(wave, singularity.neigbours.back, neighbour);
                break;

            case Constants.Up:
                RemoveImpossibleStates(wave, singularity.neigbours.up, neighbour);
                break;

            case Constants.Down:
                RemoveImpossibleStates(wave, singularity.neigbours.down, neighbour);
                break;
        }

    }

    private void RemoveImpossibleStates(Wave wave, List<int> possible_states, Superposition neighbour)
    {
        var toRemove = new List<Module>();
        foreach (var state in neighbour.States)
        {
            if (possible_states.Contains(state.hash)) continue;
            toRemove.Add(state);
        }

        foreach (var invalid_state in toRemove)
        {
            neighbour.States.Remove(invalid_state);
        }

        if (neighbour.States.Count == 1)
        {
            CreateDebugMesh(wave, neighbour.Singularity);
        }

        if(neighbour.States.Count < 1)
        {
            Debug.Log("Impossible state");
        }

    }

    // GUI

    private void OnDrawGizmos()
    {
        if (wave == null) return;

        var pos = Vector3.zero;

        Gizmos.color = Color.white;

        for (var i = 0; i < wave.Superpositions.Count(); i++)
        {
            pos = Chunk.GetCellPositionByIndex(i);
            Gizmos.DrawWireCube(pos, Vector3.one);
            Handles.Label(pos, $"{wave.Superpositions[i].GetEntropy()}");
            //Handles.Label(pos, $"{i}");
        }

        List<Color> colors = new List<Color>
        {
            Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.black
        };

        var n = Chunk.GetNeighboursByIndex(100);
        for (var i = 0; i < n.Count; i++)
        {
            if (i == -1) continue;


            Gizmos.color = colors[i];

            pos = Chunk.GetCellPositionByIndex(n[i]);
            Gizmos.DrawWireCube(pos, Vector3.one * 0.75f);
        }
    }
}

