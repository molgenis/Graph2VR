using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using VDS.RDF;

public class NodeMenu : MonoBehaviour
{
  public Graph graph;
  private CircleMenu cm = null;
  private Node node = null;
  private Edge edge = null;

  public GameObject controlerModel;
  public SteamVR_Action_Boolean clickAction = null;
  public string subMenu = "";
  public GameObject limitSlider;

  public void Start()
  {
    cm = GetComponent<CircleMenu>();
  }

  public void Update()
  {
    if (clickAction.GetStateDown(SteamVR_Input_Sources.LeftHand) == true)
    {
      Close();
    }
  }

  public void PopulateIncomingMenu()
  {
    Dictionary<string, System.Tuple<string, int>> set = graph.GetIncomingPredicats(node.GetURIAsString());
    if (set != null)
    {
      limitSlider.SetActive(true);

      foreach (KeyValuePair<string, System.Tuple<string, int>> item in set)
      {
        Color color = Color.gray;
        string label = item.Value.Item1;
        if (label == "")
        {
          label = graph.GetShortName(item.Key);
          color = Color.gray * 0.75f;
        }
        cm.AddButton(label, item.Key, color, () =>
        {
          graph.ExpandGraph(node, item.Key, false);
        }, item.Value.Item2);
      }
    }
  }

  public void PopulateOutgoingMenu()
  {
    Dictionary<string, System.Tuple<string, int>> set = graph.GetOutgoingPredicats(node.GetURIAsString());
    if (set != null)
    {
      limitSlider.SetActive(true);

      foreach (KeyValuePair<string, System.Tuple<string, int>> item in set)
      {
        Color color = Color.gray;
        string label = item.Value.Item1;
        if (label == "")
        {
          label = graph.GetShortName(item.Key);
          color = Color.gray * 0.75f;
        }
        cm.AddButton(label, item.Key, color, () =>
        {
          graph.ExpandGraph(node, item.Key, true);
        }, item.Value.Item2);
      }
    }
  }

  public void PopulateNodeMenu()
  {
    limitSlider.SetActive(false);
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

    cm.AddButton("Close node", new Color(1, 0.5f, 0.5f) / 2, () =>
      {
        graph.Remove(node);
        Close();
      });

    cm.AddButton("Add new Node (not implemented currently)", Color.grey, () =>
    {
      Close();
    });

    if (node.IsVariable)
    {
      cm.AddButton("Undo conversion", Color.blue / 2, () =>
      {
        node.UndoConversion();
        PopulateNode(node);
      });
      cm.AddButton("Rename", Color.red / 2, () => { KeyboardHandler.instance.Open(node); });
    }
    else
    {
      cm.AddButton("Convert to Variable", Color.blue / 2, () =>
      {
        node.MakeVariable();
        PopulateNode(node);
      });
    }

  }

  public void PopulateGraphMenu()
  {
    cm.AddButton("Layout: Force Directed 3D", Color.green / 2, () =>
    {
      graph.SwitchLayout<FruchtermanReingold>();
      graph.layout.CalculateLayout();
    });

    cm.AddButton("Layout: Force Directed 2D", Color.green / 2, () =>
    {
      graph.SwitchLayout<SpatialGrid2D>();
      graph.layout.CalculateLayout();
    });

    cm.AddButton("Layout: Hierarchy (na)", Color.green / 2, () =>
    {
      Close();
    });

    cm.AddButton("Auto layout", Color.yellow / 2, () =>
    {
      graph.layout.CalculateLayout();
    });

    cm.AddButton("Create Graph (na)", Color.green / 2, () =>
    {
      // Open keyboard
      // Request uri / label string
      // Query and create graph with that string
      Close();
    });


    cm.AddButton("Close Graph", new Color(1, 0.5f, 0.5f) / 2, () =>
    {
      graph.Remove();
      Close();
    });

    if (graph.subGraphs.Count > 0)
    {
      cm.AddButton("Close all child graphs", new Color(1, 0.5f, 0.5f) / 2, () =>
      {
        graph.RemoveSubGraphs();
        Close();
      });
    }
    if (graph.parentGraph != null && graph.creationQuery != "")
    {
      cm.AddButton("Close sibling graphs", new Color(1, 0.5f, 0.5f) / 2, () =>
      {
        graph.RemoveGraphsOfSameQuery();
        Close();
      });
    }

  }

  public void PopulateSettingsMenu()
  {

  }

  public void PopulateNode(Object input)
  {
    controlerModel.SetActive(false);
    limitSlider.SetActive(false);
    cm.Close();
    KeyboardHandler.instance.Close();
    node = input as Node;
    graph = node.graph;

    if (subMenu != "")
    {
      PopulateNodeDisplaySubMenus(input);
    }
    else
    {
      PopulateNodeDisplayMainMenu(input);
    }
    cm.ReBuild();
  }

  private void PopulateNodeDisplayMainMenu(Object input)
  {
    cm.AddButton("List incoming predicates", Color.green / 2, () =>
    {
      subMenu = "Incoming";
      cm.Close();
      PopulateNode(input);
    });
    if (node.graphNode.NodeType == NodeType.Uri || node.graphNode.NodeType == NodeType.Variable)
    {
      cm.AddButton("List outgoing predicates", Color.green / 2, () =>
      {
        subMenu = "Outgoing";
        cm.Close();
        PopulateNode(input);
      });
    }
    cm.AddButton("Settings", Color.yellow / 2, () =>
    {
      subMenu = "Settings";
      cm.Close();
      PopulateNode(input);
    });
    cm.AddButton("Node operations", Color.red / 2, () =>
    {
      subMenu = "Node";
      cm.Close();
      PopulateNode(input);
    });
    cm.AddButton("Graph operations", Color.red / 2, () =>
    {
      subMenu = "Graph";
      cm.Close();
      PopulateNode(input);
    });
  }

  public void PopulateNodeDisplaySubMenus(Object input)
  {
    // We are in a sub menu
    cm.AddButton("Back", Color.blue / 2, () =>
    {
      subMenu = "";
      cm.Close();
      PopulateNode(input);
    });

    if (node != null)
    {
      if (subMenu == "Incoming")
      {
        PopulateIncomingMenu();
      }
      if (subMenu == "Outgoing")
      {
        PopulateOutgoingMenu();
      }
    }
    if (subMenu == "Node")
    {
      PopulateNodeMenu();
    }
    if (subMenu == "Graph")
    {
      PopulateGraphMenu();
    }
    if (subMenu == "Settings")
    {
      PopulateSettingsMenu();
    }
  }

  public void PopulateEdge(Object input)
  {
    KeyboardHandler.instance.Close();
    limitSlider.SetActive(false);
    edge = input as Edge;
    graph = edge.graph;

    if (subMenu != "")
    {
      PopulateEdgeDisplaySubMenus(input);
    }
    else
    {
      PopulateEdgeDisplayMainMenu(input);
    }
    cm.ReBuild();
  }

  public void PopulateOrderByMenu()
  {
    int count = 1;

    foreach (DictionaryEntry order in graph.orderBy)
    {
      string label = count + " - " + order.Key;
      cm.AddButton(label, label, Color.green / 2, () =>
      {
        graph.orderBy.Remove(order.Key);
        cm.Close();
        PopulateEdge(edge);
      }, order.Value.ToString(), () =>
      {
        graph.orderBy[order.Key] = order.Value.ToString() == "ASC" ? "DESC" : "ASC";
        cm.Close();
        PopulateEdge(edge);
      });
      count++;
    }

    HashSet<string> addNameList = SelectedVariableNames();
    foreach (DictionaryEntry order in graph.orderBy)
    {
      addNameList.Remove(order.Key.ToString());
    }

    foreach (string variable in addNameList)
    {
      cm.AddButton("Order by " + variable, Color.gray / 2, () =>
        {
          graph.orderBy.Remove(variable);
          graph.orderBy.Add(variable, "ASC");
          cm.Close();
          PopulateEdge(edge);
        });
    }
  }

  private HashSet<string> SelectedVariableNames()
  {
    HashSet<string> variables = new HashSet<string>();
    List<Edge> selected = graph.selection.FindAll((edge) => edge.IsVariable || edge.displayObject.IsVariable || edge.displaySubject.IsVariable);
    foreach (Edge edge in selected)
    {
      if (edge.IsVariable) variables.Add(edge.variableName);
      if (edge.displayObject.IsVariable) variables.Add(edge.displayObject.label);
      if (edge.displaySubject.IsVariable) variables.Add(edge.displaySubject.label);
    }
    return variables;
  }

  private bool GraphHasSelectedVariable()
  {
    return graph.selection.Find((edge) => edge.IsVariable || edge.displayObject.IsVariable || edge.displaySubject.IsVariable) != null;
  }

  private void PopulateEdgeDisplayMainMenu(Object input)
  {
    controlerModel.SetActive(false);
    cm.Close();
    if (GraphHasSelectedVariable())
    {
      cm.AddButton("Order By", Color.white / 2, () =>
      {
        subMenu = "OrderBy";
        cm.Close();
        PopulateEdge(input);
      });
    }
    if (edge.IsVariable)
    {
      cm.AddButton("Undo conversion", Color.blue / 2, () =>
      {
        graph.orderBy.Remove(edge.variableName);
        edge.UndoConversion();
        PopulateEdge(input);
      });
      cm.AddButton("Rename", Color.red / 2, () =>
      {
        KeyboardHandler.instance.Open(edge);
      });
    }
    else
    {
      if (!edge.IsVariable)
      {
        cm.AddButton("Convert to Variable", Color.blue / 2, () =>
        {
          edge.MakeVariable();
          edge.Select();
          PopulateEdge(input);
        });
      }
    }

    if (edge.IsSelected)
    {
      limitSlider.SetActive(true);
      cm.AddButton("Remove selection", Color.yellow / 2, () =>
      {
        graph.orderBy.Remove(edge.variableName);
        edge.Deselect();
        PopulateEdge(input);
      });
      cm.AddButton("Query similar patterns", Color.yellow / 2, () =>
      {
        graph.QuerySimilarPatternsMultipleLayers();
      });
      cm.AddButton("Query similar patterns (single layer)", Color.yellow / 2, () =>
      {
        graph.QuerySimilarPatternsSingleLayer();
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


    cm.AddButton("Settings", Color.yellow / 2, () =>
    {
      subMenu = "Settings";
      cm.Close();
      PopulateEdge(input);
    });
    cm.AddButton("Graph operations", Color.red / 2, () =>
    {
      subMenu = "Graph";
      cm.Close();
      PopulateEdge(input);
    });
  }

  private void PopulateEdgeDisplaySubMenus(Object input)
  {
    // We are in a sub menu
    cm.AddButton("Back", Color.blue / 2, () =>
    {
      subMenu = "";
      cm.Close();
      PopulateEdge(input);
    });

    if (subMenu == "Graph")
    {
      PopulateGraphMenu();
    }
    if (subMenu == "Settings")
    {
      PopulateSettingsMenu();
    }
    if (subMenu == "OrderBy")
    {
      PopulateOrderByMenu();
    }
  }

  public void Close()
  {
    limitSlider.SetActive(false);
    node = null;
    subMenu = "";
    edge = null;
    graph = null;
    if (cm != null)
    {
      cm.Close();
      KeyboardHandler.instance.Close();
      controlerModel.SetActive(true);
    }
  }

  public void Clear()
  {
    node = null;
    subMenu = "";
    edge = null;
    graph = null;
    if (cm != null)
    {
      cm.Close();
    }
  }
}
