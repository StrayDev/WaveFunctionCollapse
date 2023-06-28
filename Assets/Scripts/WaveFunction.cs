
// System
using System.Collections.Generic;

// Unity
using UnityEngine;

public class WaveFunction
{
    public List<int>[] possibleStates;
    public bool[] collapsed;

    private readonly int MaxIndex;
    private readonly int NumCells;

    public WaveFunction(int size, List<int> states)
    {
        NumCells = size;
        MaxIndex = NumCells - 1;

        collapsed = new bool[NumCells];
        possibleStates = new List<int>[NumCells];

        // make all possibilities possible
        for (var i = 0; i < NumCells; i++)
        {
            possibleStates[i] = new List<int>();
            possibleStates[i].AddRange(states);
        }
    }

    public bool IsCollapsed()
    {
        for (int i = 0; i < NumCells; i++)
        {
            if (!collapsed[i]) return false;
        }
        return true;
    }

    public void CollapseCell(int index, int state)
    {
        possibleStates[index].Clear();
        possibleStates[index].Add(state);
        collapsed[index] = true;
    }

    private int tempIndex;

    public int SelectCellToCollapse()
    {
        // need to find the lowest entropy 
        if (TryGetLowestEnropyIndex(out var index))
        {
            return index;
        }

        // otherwise random  
        tempIndex = -1;
        while (true)
        {
            tempIndex = Random.Range(0, MaxIndex);
            if (!collapsed[tempIndex]) return tempIndex;
        }
    }

    private bool TryGetLowestEnropyIndex(out int index)
    {
        var max = MaxIndex;
        var lowest = max;

        index = -1;

        for (var i = 0; i < possibleStates.Length; i++)
        {
            // skip collapsed cells
            if (collapsed[i]) continue;

            var entropy = possibleStates[i].Count;

            // track the lowest entropy 
            if (entropy < lowest && entropy != 0)
            {
                lowest = entropy;
                index = i;
            }
        }

        if (lowest == max) return false;

        return true;
    }

    // prevents stack overflow
    private int[] adjacentCells;
    private int neighbourIndex;
    //private List<int> possibleNeighbourStates;

    public void UpdateNeighboursStateRecursive(int index, Chunk chunk, Dictionary<int, Module> modules)
    {
        // use the index to get the cell neighbours
        adjacentCells = chunk.GetCellNeighbourIndices(index);

        // cycle all adjacent
        for (var side = 0; side < adjacentCells.Length; side++)
        {
            neighbourIndex = adjacentCells[side];

            // TEMP FIX FOR CELLS OUTSIDE OF THE CHUNK
            if (neighbourIndex < 0 || neighbourIndex > MaxIndex) continue;

            // only update non collapsed
            if (!collapsed[neighbourIndex])
            {
                possibleNeighbourStates = GetPossibleNeighbourStates(index, side, modules);

                RemoveImpossibleStates(ref possibleStates[neighbourIndex], possibleNeighbourStates);

                var numPosibilities = possibleStates[neighbourIndex].Count;

                // temp break for imposible states
                if(numPosibilities < 1)
                {
                    // this should not happen
                    CollapseCell(neighbourIndex, 1676256714);
                    break;
                }

                // update neighbour
                possibleStates[neighbourIndex] = possibleNeighbourStates;
                if (possibleStates[neighbourIndex].Count < 2)
                {
                    CollapseCell(neighbourIndex, possibleStates[neighbourIndex][0]);
                    collapsed[neighbourIndex] = true;
                }

                // mark index complete and recurse
                UpdateNeighboursStateRecursive(neighbourIndex, chunk, modules);
            }

        }
    }

    private List<int> ToRemove = new List<int>();

    private void RemoveImpossibleStates(ref List<int> list, List<int> possibleNeighbourStates)
    {
        ToRemove.Clear();

        foreach (var state in list)
        {
            if (!possibleNeighbourStates.Contains(state)) ToRemove.Add(state);
        }

        foreach (var r in ToRemove) list.Remove(r);
    }

    private List<int> possibleNeighbourStates = new List<int>();
    private Module CellModule;

    private List<int> GetPossibleNeighbourStates(int index, int side, Dictionary<int, Module> modules)
    {
        possibleNeighbourStates.Clear();

        foreach (var hash in possibleStates[index])
        {
            // get the module for the central cell
            CellModule = modules[hash];

            // get the list of possible hashes for each side
            foreach (var option in CellModule.neigbours[side])
            {
                //if (possibleNeighbourStates.Contains(option)) continue;
                if (possibleNeighbourStates.Contains(option)) continue;

                possibleNeighbourStates.Add(option);
            }
        }

        return possibleNeighbourStates;
    }

/*    private bool IsAlreadyInList(List<int>[] possibleStates, int option)
    {
        for(var i = 0; i < possibleStates.Length; i++)
        {
            possibleStates[i].Contains(option);
        }
    }*/

    public List<int> GetPossibleStates(int index)
    {
        return possibleStates[index];
    }


}

