using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class World
{
    public string Name = "New World";
    public string FilePath;
    
    public List<Area> Areas { get; private set; }

    public World()
    {
        Areas = new List<Area>();
        Areas.Add(new Area("New Area"));
    }
}
