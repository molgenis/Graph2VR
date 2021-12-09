using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NodeMenu : MonoBehaviour
{
  public Graph graph;
  private CircleMenu cm = null;
  public bool isOutgoingLink = true;
  private Dictionary<string, System.Tuple<string, int>> set;
  private Node node = null;
  private Edge edge = null;

  public GameObject controlerModel;

  public void Start()
  {
    cm = GetComponent<CircleMenu>();
  }

  public void GetPredicats()
  {
    if (node != null)
    {
      if (isOutgoingLink)
      {
        set = graph.GetOutgoingPredicats(node.GetURIAsString());
      }
      else
      {
        set = graph.GetIncomingPredicats(node.GetURIAsString());
      }
    }
  }

  public void Update()
  {

    if (ControlerInput.instance.triggerLeft)
    {
      Close();
    }
  }

  public void PopulateNode(Object input)
  {
    KeyboardHandler.instance.Close();
    node = input as Node;
    graph = node.graph;
    if (node.IsVariable)
    {
      Close();
      controlerModel.SetActive(false);
      cm.AddButton("Undo conversion", Color.blue / 2, () =>
      {
        node.UndoConversion();
        PopulateNode(input);
      });
      cm.AddButton("Rename", Color.red / 2, () => { KeyboardHandler.instance.Open(node); });
      cm.ReBuild();
    }
    else
    {
      GetPredicats();
      Close();
      controlerModel.SetActive(false);

      if (set != null)
      {
        if (isOutgoingLink)
        {
          cm.AddButton("List incoming predicts", Color.blue / 2, () =>
          {
            isOutgoingLink = false;
            PopulateNode(input);
          });
        }
        else
        {
          cm.AddButton("List outgoing predicts", Color.blue / 2, () =>
          {
            isOutgoingLink = true;
            PopulateNode(input);
          });
        }

        foreach (KeyValuePair<string, System.Tuple<string, int>> item in set)
        {
          //Debug.Log("k: " + item.Key + " v1: " + item.Value.Item1 + " v2: " + item.Value.Item2);
          Color color = Color.gray;
          string label = item.Value.Item1;
          if (label == "")
          {
            label = item.Key;
            color = Color.gray * 0.75f;
          }
          // TODO: add qname als alt.

          cm.AddButton(label, color, () =>
          {
            graph.ExpandGraph(node, item.Key, isOutgoingLink);
            Close();
          }, item.Value.Item2);
        }
      }

      if (!node.IsVariable)
      {
        cm.AddButton("Convert to Variable", Color.blue / 2, () =>
        {
          node.MakeVariable();
          PopulateNode(input);
        });
      }

      if (node.uri != "")
      {
        cm.AddButton("Collapse Incoming", new Color(1, 0.5f, 0.5f) / 2, () =>
        {
          graph.CollapseIncomingGraph(node);
        });
        cm.AddButton("Collapse Outgoing", new Color(1, 0.5f, 0.5f) / 2, () =>
        {
          graph.CollapseOutgoingGraph(node);
        });
        cm.AddButton("Collapse All", new Color(1, 0.5f, 0.5f) / 2, () =>
        {
          graph.CollapseGraph(node);
        });
      }

      cm.AddButton("Close", Color.red / 2, () =>
      {
        graph.RemoveNode(node);
        Close();
      });

      cm.ReBuild();
    }
  }

  public void PopulateEdge(Object input)
  {
    KeyboardHandler.instance.Close();
    edge = input as Edge;
    if (edge.IsVariable)
    {
      Close();
      controlerModel.SetActive(false);
      cm.AddButton("Undo conversion", Color.blue / 2, () =>
      {
        edge.UndoConversion();
        PopulateEdge(input);
      });

      if (edge.IsSelected)
      {
        cm.AddButton("Remove selection", Color.yellow / 2, () =>
        {
          edge.Deselect();
          PopulateEdge(input);
        });
        cm.AddButton("Query similar patterns", Color.yellow / 2, () =>
        {
          graph.QuerySimilarPatterns();
        });
      }
      else
      {
        cm.AddButton("Select triple", Color.yellow / 2, () =>
        {
          edge.Select();
          PopulateEdge(input);
        });
      }
      cm.AddButton("Rename", Color.red / 2, () => { KeyboardHandler.instance.Open(edge); });
      cm.ReBuild();
    }
    else
    {
      Close();
      controlerModel.SetActive(false);

      if (!edge.IsVariable)
      {
        cm.AddButton("Convert to Variable", Color.blue / 2, () =>
        {
          edge.MakeVariable();
          PopulateEdge(input);
        });
      }

      if (edge.IsSelected)
      {
        cm.AddButton("Remove selection", Color.yellow / 2, () =>
        {
          edge.Deselect();
          PopulateEdge(input);
        });
        cm.AddButton("Query similar patterns", Color.yellow / 2, () =>
        {
          graph.QuerySimilarPatterns();
        });

      }
      else
      {
        cm.AddButton("Select triple", Color.yellow / 2, () =>
        {
          edge.Select();
          PopulateEdge(input);
        });
      }

      cm.ReBuild();
    }
  }

  public void Close()
  {
    if (cm != null)
    {
      cm.Close();
      KeyboardHandler.instance.Close();
      controlerModel.SetActive(true);
    }
  }
}
