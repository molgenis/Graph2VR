using System;
using System.Collections.Generic;
using UnityEngine;
using VDS.RDF;
using VDS.RDF.Query;

public class NodeMenu : BaseMenu
{
  protected Dictionary<string, Tuple<string, int>> labelAndCountByUri = null;

  public void PopulateNode(UnityEngine.Object input)
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

  public void PopulateOutgoingMenu()
  {
    if (populateMenuState == PopulateMenuState.unloaded)
    {
      graph.GetOutgoingPredicats(node.GetURIAsString(), PopulateMenuCallback);
      populateMenuState = PopulateMenuState.loading;
    }

    if (populateMenuState == PopulateMenuState.loaded)
    {
      DrawDelayedMenuButtons(true);
    }
    else
    {
      cm.AddButton("Loading...", new Color(1, 1, 1) / 2, () => { });
    }
  }

  public void PopulateIncomingMenu()
  {
    if (populateMenuState == PopulateMenuState.unloaded)
    {
      graph.GetIncomingPredicats(graph.RealNodeValue(node.graphNode), PopulateMenuCallback);
      populateMenuState = PopulateMenuState.loading;
    }

    if (populateMenuState == PopulateMenuState.loaded)
    {
      DrawDelayedMenuButtons(false);
    }
    else
    {
      cm.AddButton("Loading...", new Color(1, 1, 1) / 2, () => { });
    }

  }

  private void DrawDelayedMenuButtons(bool isOutgoingLink)
  {
    //draw buttons
    if (labelAndCountByUri != null)
    {
      limitSlider.SetActive(true);
      foreach (KeyValuePair<string, Tuple<string, int>> item in labelAndCountByUri)
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
          graph.ExpandGraph(node, item.Key, isOutgoingLink);
        }, number: item.Value.Item2);
      }
    }
  }

  private void PopulateMenuCallback(SparqlResultSet results, object state)
  {
    try
    {
      UnityMainThreadDispatcher.Instance().Enqueue(() =>
      {
        labelAndCountByUri = Utils.GetPredicatsList(results);
        populateMenuState = PopulateMenuState.loaded;
        PopulateNode(node);
      });
    }
    catch (Exception e)
    {
      Debug.Log("error: " + e.Message);
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
      cm.AddButton("Show/hide info", new Color(1, 0.5f, 0.5f) / 2, () =>
      {
        node.ToggleInfoPanel();
      });
    }

    cm.AddButton("Close node", new Color(1, 0.5f, 0.5f) / 2, () =>
    {
      graph.RemoveNode(node, true);
      Close();
    });

  }

  private void PopulateNodeDisplayMainMenu(UnityEngine.Object input)
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
    cm.AddButton("Close/Remove operations", Color.red / 2, () =>
    {
      subMenu = "Node";
      cm.Close();
      PopulateNode(input);
    });
    cm.AddButton("Graph operations", Color.yellow / 2, () =>
    {
      subMenu = "Graph";
      cm.Close();
      PopulateNode(input);
    });

    if (node.lockPosition)
    {
      cm.AddButton("Unlock position", new Color(0.5f, 1f, 0.5f) / 2, () =>
      {
        LeanTween.cancel(node.gameObject);
        LeanTween.value(node.gameObject, 0.2f, 0.4f, 0.3f).setOnUpdate(value => node.transform.Find("Nail").GetComponent<NailRotation>().offset = value).setOnComplete(() =>
           {
             node.LockPosition = false;
             cm.Close();
             PopulateNode(input);
           });
      });
    }
    else
    {
      cm.AddButton("Lock position", new Color(0.5f, 1f, 0.3f) / 2, () =>
      {
        LeanTween.cancel(node.gameObject);
        LeanTween.value(node.gameObject, 0.4f, 0.2f, 0.5f).setOnUpdate(value => node.transform.Find("Nail").GetComponent<NailRotation>().offset = value);
        node.LockPosition = true;
        cm.Close();
        PopulateNode(input);
      });
    }

    if (node.IsVariable)
    {
      cm.AddButton("Undo variable conversion", Color.blue / 2, () =>
      {
        node.UndoConversion();
        PopulateNode(node);
      });
      cm.AddButton("Rename", Color.red / 2, () => { KeyboardHandler.instance.Open(node); });
      cm.AddButton("Search for existing node", Color.blue / 2, () =>
      {
        Main.instance.FindClosestGraphOrCreateNewGraph(transform.position).AddNodeFromDatabase(node);
        Close();
      });
    }
    else
    {
      cm.AddButton("Convert to Variable", ColorSettings.instance.variableColor / 2, () =>
      {
        node.MakeVariable();
        PopulateNode(node);
      });
    }
  }

  public void PopulateNodeDisplaySubMenus(UnityEngine.Object input)
  {
    // We are in a sub menu
    cm.AddButton("Back", Color.blue / 2, () =>
    {
      subMenu = "";
      populateMenuState = PopulateMenuState.unloaded;
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
  }
}
