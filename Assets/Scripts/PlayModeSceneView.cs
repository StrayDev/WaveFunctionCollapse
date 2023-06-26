
// Unity
using UnityEditor;
using UnityEngine;

//
[InitializeOnLoad]
public class PlayModeSceneView : MonoBehaviour
{
    static PlayModeSceneView()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            EditorApplication.ExecuteMenuItem("Window/General/Scene");
        }
    }
}

