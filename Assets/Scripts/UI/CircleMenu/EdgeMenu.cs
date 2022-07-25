using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeMenu : BaseMenu
{
  public void PopulateEdge(UnityEngine.Object input)
  {
    KeyboardHandler.instance.Close();
    controlerModel.SetActive(false);
    limitSlider.SetActive(false);
    cm.Close();
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

  private void PopulateEdgeDisplayMainMenu(UnityEngine.Object input)
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
    cm.AddButton("Graph operations", Color.yellow / 2, () =>
    {
      subMenu = "Graph";
      cm.Close();
      PopulateEdge(input);
    });

    cm.AddButton("Close Edge", new Color(1, 0.5f, 0.5f) / 2, () =>
    {
      graph.RemoveEdge(edge);
      Close();
    });

    if (edge.IsVariable)
    {
      cm.AddButton("Undo variable conversion", Color.blue / 2, () =>
      {
        graph.orderBy.Remove(edge.variableName);
        edge.UndoConversion();
        PopulateEdge(input);
      });
      cm.AddButton("Rename variable", Color.red / 2, () =>
      {
        Utils.GetStringFromVRKeyboard((string name) =>
        {
          edge.SetVariableName(name);
        }
        , node.GetLabel(), "Enter a variable name...");
      });
    }
    else
    {
      if (!edge.IsVariable)
      {
        cm.AddButton("Convert to Variable and select", ColorSettings.instance.variableColor / 2, () =>
        {
          edge.MakeVariable();
          edge.Select();
          PopulateEdge(input);
        });
      }
    }

  }

  private void PopulateEdgeDisplaySubMenus(UnityEngine.Object input)
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
    if (subMenu == "OrderBy")
    {
      PopulateOrderByMenu();
    }
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
}
