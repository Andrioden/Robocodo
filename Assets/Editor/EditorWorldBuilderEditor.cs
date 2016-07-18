using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(DemoWorldBuilder))]
public class EditorWorldBuilderEditor : Editor
{
    override public void OnInspectorGUI()
    {
        DrawDefaultInspector();
        DemoWorldBuilder demoWorldBuilder = (DemoWorldBuilder)target;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Build"))
        {
            demoWorldBuilder.SetupDemoWorld();
        }
        if (GUILayout.Button("Destroy"))
        {
            demoWorldBuilder.DestroyDemoWorld();
        }
        GUILayout.EndHorizontal();
    }
}
