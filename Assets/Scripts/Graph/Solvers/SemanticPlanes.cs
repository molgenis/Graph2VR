using System.Collections.Generic;
using UnityEngine;
using VDS.RDF;
using VDS.RDF.Query;

public class SemanticPlanes : BaseLayoutAlgorithm
{
  public SparqlResultSet variableNameLookup;
  public Quaternion lookDirection = Quaternion.identity;

  public string GetIdentifier(Node node)
  {
    string identifier = node.uri;
    if (node.IsVariable || node.uri == "" || node.uri == null)
    {
      identifier = node.GetQueryLabel();
    }
    return identifier;
  }

  public override void CalculateLayout()
  {
    Vector3 normal = lookDirection * Vector3.forward;
    Plane plane = new Plane(normal, transform.position);

    // Selected edges is main graph
    Dictionary<string, Vector3> uriToPosition = new Dictionary<string, Vector3>();
    if (parentGraph != null)
    {
      foreach (Edge edge in parentGraph.selection)
      {
        string subjectName = GetIdentifier(edge.displaySubject);
        string objectName = GetIdentifier(edge.displayObject);

        if (!uriToPosition.ContainsKey(subjectName))
          uriToPosition.Add(subjectName, edge.displaySubject.transform.localPosition);
        if (!uriToPosition.ContainsKey(objectName))
          uriToPosition.Add(objectName, edge.displayObject.transform.localPosition);
      }
    }

    /*
    foreach (var item in uriToPosition)
    {
      Debug.Log(item.Key);
    }
    */

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
            if (col.Value == null) continue;
            if (node.uri.ToString() == col.Value.ToString())
            {
              variableName = "?" + col.Key;
              stop = true;
              break;
            }
          }

          if (variableName != "" && uriToPosition.TryGetValue(variableName, out position))
          {
            node.transform.localPosition = position;
          }
          else if (node.graphNode.NodeType == NodeType.Literal)
          {
            if (uriToPosition.TryGetValue(node.GetQueryLabel(), out position))
            {
              node.transform.localPosition = position;
            }
          }
          ///
        }
      }

      // flatten graph
      node.transform.position = plane.ClosestPointOnPlane(node.transform.position);
    }
  }

  public override void Stop() { }
}

