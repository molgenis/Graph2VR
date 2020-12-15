
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Main))]
public class MainHelper : Editor
{
    string text = "select distinct <http://dbpedia.org/resource/Biobank> as ?s ?p ?o where { <http://dbpedia.org/resource/Biobank> ?p ?o } LIMIT 100";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Main main = (Main)target;

        text = EditorGUILayout.TextArea(text, GUILayout.Height(100));

        if (GUILayout.Button("SendQuery")) {
            Graph.instance.SendQuery(text);
        }
    }
}
