
// Unity 
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(TileSetJsonSaveSystem))]
public class TileSetJsonSaveSystemEditor : Editor
{

    public override void OnInspectorGUI()
    {
        var tool = (TileSetJsonSaveSystem)target;

        base.OnInspectorGUI();

        GUILayout.Space(10);
        if (GUILayout.Button("Save Tileset"))
        {
            tool.SaveTilesetToFile();
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Load Tileset"))
        {
            tool.LoadTilesetFromFile();
        }
    }

}