using UnityEngine;

public class DebugDrawGrid : MonoBehaviour
{
    void OnDrawGizmos()
    {
        // Get the camera's projection matrix
        Matrix4x4 projMatrix = Camera.main.projectionMatrix;

        // Calculate the frustum corners in clip space
        Vector4[] corners = new Vector4[4];
        corners[0] = new Vector4(-1, -1, 0, 1);
        corners[1] = new Vector4(1, -1, 0, 1);
        corners[2] = new Vector4(1, 1, 0, 1);
        corners[3] = new Vector4(-1, 1, 0, 1);
        for (int i = 0; i < 4; i++)
        {
            corners[i] = projMatrix.inverse * corners[i];
            corners[i] /= corners[i].w;
        }

        // Project the corners onto the Y=0 plane
        Vector3[] corners3D = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            corners3D[i] = new Vector3(corners[i].x, 0, corners[i].z);
        }

        // Draw the bounds
        Gizmos.color = Color.green;
        Gizmos.DrawLine(corners3D[0], corners3D[1]);
        Gizmos.DrawLine(corners3D[1], corners3D[2]);
        Gizmos.DrawLine(corners3D[2], corners3D[3]);
        Gizmos.DrawLine(corners3D[3], corners3D[0]);
    }
}

