using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Superposition
{
    public List<Module> States;

    public int GetEntropy()
    {
        return States.Count;
    }

    public Module Singularity => States.First();
}
