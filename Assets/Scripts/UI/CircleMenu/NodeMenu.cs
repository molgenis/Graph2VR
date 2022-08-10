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
      cm.AddButton("Loading...", grayColor, () => { });
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
      cm.AddButton("Loading...", grayColor, () => { });
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
      cm.AddButton(Icon("\uF057") + "Collapse Incoming", dangerColor, () =>
      {
        graph.CollapseIncomingGraph(node);
      });
      cm.AddButton(Icon("\uF057") + "Collapse Outgoing", dangerColor, () =>
      {
        graph.CollapseOutgoingGraph(node);
      });
      cm.AddButton(Icon("\uF057") + "Collapse All", dangerColor, () =>
      {
        graph.CollapseGraph(node);
      });
    }

    cm.AddButton(Icon("\uF057") + "Close node", dangerColor, () =>
    {
      graph.RemoveNode(node, true);
      Close();
    });

  }

  private void PopulateNodeDisplayMainMenu(UnityEngine.Object input)
  {
    cm.AddButton(Icon("\uF060") + "List incoming predicates", okColor, () =>
    {
      subMenu = "Incoming";
      cm.Close();
      PopulateNode(input);
    });
    if (node.graphNode.NodeType == NodeType.Uri || node.graphNode.NodeType == NodeType.Variable)
    {
      cm.AddButton(Icon("\uF061") + "List outgoing predicates", okColor, () =>
      {
        subMenu = "Outgoing";
        cm.Close();
        PopulateNode(input);
      });
    }
    cm.AddButton(Icon("\uF057") + "Close/Remove operations", closeRemoveColor, () =>
    {
      subMenu = "Node";
      cm.Close();
      PopulateNode(input);
    });
    cm.AddButton(Icon("\uF05A") + "Show/hide info", defaultMenuColor, () =>
    {
      node.ToggleInfoPanel();
    });
    cm.AddButton(Icon("\uF1E0") + "Graph operations", warningColor, () =>
    {
      subMenu = "Graph";
      cm.Close();
      PopulateNode(input);
    });

    if (node.lockPosition)
    {
      cm.AddButton(Icon("\uF023") + "Unpin position", okColor, () =>
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
      cm.AddButton(Icon("\uF3C1") + "Pin position", okColor, () =>
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
      cm.AddButton(Icon("\uF111") + "Undo variable conversion", defaultMenuColor, () =>
      {
        node.UndoConversion();
        PopulateNode(node);
      });
      cm.AddButton(Icon("\uF11C") + "Rename variable", dangerColor, () =>
      {
        Utils.GetStringFromVRKeyboard((string label) =>
        {
          node.SetLabel(label);
        }
        , node.GetLabel(), "Enter a variable name...");
      });
      cm.AddButton(Icon("\uF11C") + "Search for existing node", defaultMenuColor, () =>
      {
        Main.instance.FindClosestGraphOrCreateNewGraph(transform.position).AddNodeFromDatabase(node);
        Close();
      });
    }
    else
    {
      cm.AddButton(Icon("\uF128") + "Convert to Variable", ColorSettings.instance.variableColor / 2, () =>
      {
        node.MakeVariable();
        PopulateNode(node);
      });
    }
  }

  public void PopulateNodeDisplaySubMenus(UnityEngine.Object input)
  {
    // We are in a sub menu
    cm.AddButton(Icon("\uF064") + "Back", defaultMenuColor, () =>
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
