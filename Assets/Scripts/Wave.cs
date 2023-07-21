
using System.Collections.Generic;

public class Wave
{
    public int LastCollapsedCell = -1;

    public List<Superposition> Superpositions;

    public Wave(int _size, List<Module> unobserved_state)
    {
        Superpositions = new List<Superposition>();

        for (var i = 0; i < _size; i++)
        {
            var superposition = new Superposition
            {
                States = new List<Module>(unobserved_state)
            };

            Superpositions.Add(superposition);
        }
    }
}
