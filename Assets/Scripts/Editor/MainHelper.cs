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

    if (GUILayout.Button("Send Construct Query")) {
      Main.instance.mainGraph.SendQuery(text);
    }

    if (GUILayout.Button("Send Construct Query to new Graph")) {
      Main.instance.mainGraph.SendQuery(text);
      Graph graph = Main.instance.CreateGraph();
      graph.SendQuery(text);

    }

    if (GUILayout.Button("Clear")) {
      Main.instance.mainGraph.Clear();
    }

  }
}
