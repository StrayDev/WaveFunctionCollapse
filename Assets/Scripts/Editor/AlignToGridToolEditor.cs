// Unity 
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AlignToGridTool))]
public class AlignToGridToolEditor : Editor
{

    public override void OnInspectorGUI()
    {
        var tool = (AlignToGridTool)target;
        
        base.OnInspectorGUI();
        
        GUILayout.Space(10);
        if (GUILayout.Button("Align To Grid"))
        {
            tool.AlignToGrid();
        }
    }
    
}
