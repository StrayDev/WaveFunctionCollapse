//
using UnityEngine;

public class AlignToGridTool : MonoBehaviour
{
    [SerializeField] private float offset = 2;

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