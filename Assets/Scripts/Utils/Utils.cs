using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VDS.RDF;
using VDS.RDF.Query;

public class Utils
{
  static public void GetStringFromVRKeyboard(Action<string> callback, string initialValue = "", string placeHolder = "...")
  {
    Main.instance.keyboard.GetComponent<GetStringFromKeyboardHandler>().GetString(callback, initialValue, placeHolder);
  }

  static public GameObject FindClosestGraph(Vector3 position)
  {
    GameObject closest = null;
    GameObject[] graphs = GameObject.FindGameObjectsWithTag("Graph");
    float closestDistance = float.MaxValue;
    foreach (GameObject graph in graphs)
    {
      float distance = Vector3.Distance(position, graph.GetComponent<Graph>().boundingSphere.transform.position);
      if (distance < closestDistance)
      {
        closestDistance = distance;
        closest = graph;
      }
    }
    return closest;
  }

  static public Node GetPartnerNode(Node node, Edge edge)
  {
    if (IsSubjectNode(node, edge))
    {
      return edge.displayObject;
    }
    else
    {
      return edge.displaySubject;
    }
  }

  static public bool IsSubjectNode(Node node, Edge edge)
  {
    return node.graph.RealNodeValue(node.graphNode) == node.graph.RealNodeValue(edge.graphSubject);
  }

  static public Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
  {
    float u = 1 - t;
    float tt = t * t;
    float uu = u * u;
    float uuu = uu * u;
    float ttt = tt * t;

    Vector3 p = uuu * p0;
    p += 3 * uu * t * p1;
    p += 3 * u * tt * p2;
    p += ttt * p3;
    return p;
  }

  static public string GetShortLabelFromUri(string uri)
  {
    var list = uri.Split('/', '#');
    if (list.Length > 0)
    {
      return list[list.Length - 1];
    }
    else
    {
      return uri;
    }
  }

  static public Dictionary<string, Tuple<string, int>> GetPredicatsList(SparqlResultSet sparqlResults)
  {
    return sparqlResults.Aggregate(new Dictionary<string, Tuple<string, int>>(), (accum, result) =>
    {
      result.TryGetValue("p", out INode predicate);
      result.TryGetValue("count", out INode countNode);
      result.TryGetValue("label", out INode labelNode);

      string label = labelNode != null ? labelNode.ToString() : "";

      if (predicate != null)
      {
        string predicateString = predicate.ToString();
        int count = int.Parse(countNode.ToString());
        Tuple<string, int> value = new Tuple<string, int>(label, count);
        if (!accum.ContainsKey(predicateString))
        {
          accum.Add(predicateString, value);
        }
        else
        {
          // Why overwrite and not just skip?
          accum[predicateString] = value;
        }
      }
      return accum;
    });
  }
}

