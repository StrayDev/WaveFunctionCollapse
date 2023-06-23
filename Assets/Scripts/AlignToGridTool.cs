//
using UnityEngine;

public class AlignToGridTool : MonoBehaviour
{
    [SerializeField] private float width = 16;
    [SerializeField] private float offset = 2;
    [SerializeField] private float x = 0;
    [SerializeField] private float z = 0;
    
    public void AlignToGrid()
    {
        foreach (Transform obj in transform)
        {
            if (x >= width)
            {
                x = 0;
                z += offset;
            }

            x += offset;
            obj.position = new Vector3(x, 0, z);
        }
    }
}