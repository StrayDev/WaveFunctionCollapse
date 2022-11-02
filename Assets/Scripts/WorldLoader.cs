using UnityEditor;
using UnityEngine;


public class WorldLoader : MonoBehaviour
{
    [SerializeField] private World world;


    [ContextMenu("Create New World")]
    public void CreateNewWorld()
    {
        world = new World();
    }

    // test only  : todo interface into game / tool 
    [ContextMenu("Up New Chunk")] public void NewChunkUp() => world.Areas[0].CreateNewChunk(world.Areas[0].Chunks[0], Vector3.up);
    [ContextMenu("Down New Chunk")] public void NewChunkDown() => world.Areas[0].CreateNewChunk(world.Areas[0].Chunks[0], Vector3.down);
    [ContextMenu("left New Chunk")] public void NewChunkleft() => world.Areas[0].CreateNewChunk(world.Areas[0].Chunks[0], Vector3.left);
    [ContextMenu("right New Chunk")] public void NewChunkright() => world.Areas[0].CreateNewChunk(world.Areas[0].Chunks[0], Vector3.right);
    [ContextMenu("forward New Chunk")] public void NewChunkforward() => world.Areas[0].CreateNewChunk(world.Areas[0].Chunks[0], Vector3.forward);
    [ContextMenu("back New Chunk")] public void NewChunkback() => world.Areas[0].CreateNewChunk(world.Areas[0].Chunks[0], Vector3.back);

    
    
    public bool LoadWorld(string filePath)
    {
        if (LoadWorldFromFile(filePath, out World w))
        {
            return true;
        }

        return false;
    }

    private bool LoadWorldFromFile(string filepath, out World w)
    {
        w = new World(); // todo implement this function
        return false;
    }
    
    private void OnDrawGizmos()
    {
        if (world == null) return;
        
        foreach (var area in world.Areas)
        {
            foreach (var chunk in area.Chunks)
            {
                Handles.DrawWireCube(chunk.center, Chunk.Bounds);
                
                /*foreach (var cell in chunk.cells)
                {
                    Gizmos.DrawCube(cell.center, Cell.Size);
                }*/
            }
        }
    }
}