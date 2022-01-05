using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VDS.RDF.Query;

public class SemanticPlanes : BaseLayoutAlgorithm
{
  public SparqlResultSet variableNameLookup;
  public override void CalculateLayout()
  {
    Debug.Log("SemanticPlanes CalculateLayout");

    // Selected edges is main graph
    Dictionary<string, Vector3> uriToPosition = new Dictionary<string, Vector3>();
    foreach (Edge edge in parentGraph.selection)
    {
      string subjectName = edge.displaySubject.IsVariable ? edge.displaySubject.GetLabel() : edge.displaySubject.uri;
      string objectName = edge.displayObject.IsVariable ? edge.displayObject.GetLabel() : edge.displayObject.uri;
      uriToPosition.Add(subjectName, edge.displaySubject.transform.localPosition);
      uriToPosition.Add(objectName, edge.displayObject.transform.localPosition);
    }

    // Set positions we know
    foreach (Node node in graph.nodeList)
    {
      Vector3 position = Vector3.zero;
      if (uriToPosition.TryGetValue(node.uri, out position))
      {
        node.transform.localPosition = position;
      }
      else
      {
        // probably a variable
        string variableName = "";
        bool stop = false;
        foreach (SparqlResult result in variableNameLookup)
        {
          if (stop) break;
          foreach (var col in result)
          {
            if (node.uri.ToString() == col.Value.ToString())
            {
              variableName = "?" + col.Key;
              stop = true;
              break;
            }
          }
        }
        if (variableName != "" && uriToPosition.TryGetValue(variableName, out position))
        {
          node.transform.localPosition = position;
        }
        else
        {
          Debug.Log("We do not expect this to happen");
        }
      }

      // flatten graph
      node.transform.localPosition = new Vector3(node.transform.localPosition.x, node.transform.localPosition.y, 0);
    }

  }

  public override void Stop()
  {

  }
}
