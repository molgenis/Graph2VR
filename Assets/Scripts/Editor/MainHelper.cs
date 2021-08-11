﻿
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Main))]
public class MainHelper : Editor
{
    string text = "select ?s ?p ?o WHERE{<http://www.semanticweb.org/alexander/ontologies/2021/6/untitled-ontology-479#geste_children_6to7years:icteric_lipids:Collected> ?p ?o.} LIMIT 20";

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
