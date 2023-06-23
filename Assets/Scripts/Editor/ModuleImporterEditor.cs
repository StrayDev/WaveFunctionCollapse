
// Unity 
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(ModuleImporter))]
public class ModuleImporterEditor : Editor
{

    public override void OnInspectorGUI()
    {
        var tool = (ModuleImporter)target;
        
        base.OnInspectorGUI();

        GUILayout.Space(10);
        if (GUILayout.Button("Import Modules"))
        {
            tool.ImportModules();
        }
    }
    
}
