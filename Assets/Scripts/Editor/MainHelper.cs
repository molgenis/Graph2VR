
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Main))]
public class MainHelper : Editor
{
    string text = "construct { <http://dbpedia.org/resource/Biobank> ?p ?o } where { <http://dbpedia.org/resource/Biobank> ?p ?o } LIMIT 100";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Main main = (Main)target;

        text = EditorGUILayout.TextArea(text, GUILayout.Height(100));

        if (GUILayout.Button("Send Query")) {
            Graph.instance.SendQuery(text);
        }

        if (GUILayout.Button("Clear")) {
            Graph.instance.Clear();
        }

    }
}
