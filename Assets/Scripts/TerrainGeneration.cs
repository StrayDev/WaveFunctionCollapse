
// System
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditorInternal;

// Unity
using UnityEngine;

//
public class TerrainGeneration : MonoBehaviour
{

    [SerializeField] private TileSetAsset tileSetAsset;

    private List<Module> Modules => tileSetAsset.tileset.modules;

    private async void Start()
    {
        await Task.Delay(1000);

        await GenerateTerrainAsync();

        Debug.Log("Terrain Generation Complete");
    }

    private async Task GenerateTerrainAsync()
    {
        // Unobserved state has a list of all modules
        var unobserved_state = Modules;

        // Create wave containing superpositions for every cell
        var wave = new Wave(Chunk.CellCount, unobserved_state);

        // Repeat the next steps
        while (true)
        {
            // When observation fails Generation should be complete
            if (!Observation(wave)) break;

            // Propagate changes in state
            Propagation(wave);

            // Make the effect visible
            await Task.Delay(333);
        }
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
        var lowest_value = int.MaxValue;
        super_position = null;

        foreach (var cell in wave.Superpositions) 
        {
            var cell_entropy = cell.Entropy;

            // filter out collapsed superpositions
            if (cell_entropy == 1) continue;

            if (cell_entropy < lowest_value)
            {
                // record the lowest entropy and the coresponding superposition
                lowest_value = cell_entropy;
                super_position = cell;
            }
        }

        if (lowest_value == int.MaxValue)
        {
            // failed to observe, break the loop
            super_position = null;
            return false;
        }

        return true;
    }

    private void CollapseSuperPosition(Wave wave, Superposition super_position)
    {
        // not sure this is the correct approach
        var element = UnityEngine.Random.Range(0, super_position.Entropy - 1);
        var singularity = super_position.States[element];

        super_position.States.Clear();
        super_position.States.Add(singularity);

        // < < < Create GameObject from Singularity
    }

    private void Propagation(Wave wave)
    {
        // Update possible states of remaining super positions
    }

}

internal class Wave
{
    public List<Superposition> Superpositions;

    public Wave(int _size, List<Module> unobserved_state)
    {
        Superpositions = new List<Superposition>();

        for (var i = 0; i < _size; i++) 
        {
            var superposition = new Superposition{ States = unobserved_state };
            Superpositions.Add(superposition);
        }
    }
}

internal class Superposition
{
    public List<Module> States;

    public int Entropy => States.Count;
    public Module Singularity => States[0];
}