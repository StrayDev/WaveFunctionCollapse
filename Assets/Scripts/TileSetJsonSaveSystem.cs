
// System
using System.IO;

// Unity
using UnityEngine;

public class TileSetJsonSaveSystem : MonoBehaviour
{
    [Space]
    public TileSetAsset tileSetAsset;

    [Space]
    public string FileName;
    public string CustomDirectory;
    public bool UseCustomFilePath;

    public void SaveTilesetToFile()
    {
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(tileSetAsset.tileset, Newtonsoft.Json.Formatting.Indented);

        var filepath = GetDirectory() + '\\' + FileName + ".json";

        File.WriteAllText(filepath, json);

        Debug.Log($"Saved Tileset To : {filepath} ");
    }

    public void LoadTilesetFromFile()
    {
        var filepath = GetDirectory() + '\\' + FileName + ".json";

        var json = File.ReadAllText(filepath);

        tileSetAsset.tileset = Newtonsoft.Json.JsonConvert.DeserializeObject<TileSet>(json);

        Debug.Log($"Loaded Tileset from : {filepath} ");
    }

    private string GetDirectory()
    {
        return UseCustomFilePath ? CustomDirectory : Application.persistentDataPath;
    }
}
