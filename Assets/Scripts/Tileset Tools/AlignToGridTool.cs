//
using UnityEngine;

public class AlignToGridTool : MonoBehaviour
{
    [SerializeField] private float width = 8;
    [SerializeField] private float offset = 2;
    [SerializeField] private float x = 0;
    [SerializeField] private float z = 0;

    public void AlignToGrid()
    {
        var count = 1;

        foreach (Transform obj in transform)
        {
            obj.position = Vector3.zero;
        }

        foreach (Transform obj in transform)
        {
            obj.position += (Vector3.left * offset) * count;
            count++;
        }
    }
}