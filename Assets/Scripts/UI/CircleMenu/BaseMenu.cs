using System.Collections.Generic;
using UnityEngine;

public class BaseMenu : MonoBehaviour
{
  public Graph graph;
  protected CircleMenu cm = null;
  public GameObject controlerModel;
  public string subMenu = "";
  public GameObject limitSlider;
  protected Node node = null;
  protected Edge edge = null;

  protected enum PopulateMenuState { unloaded, loading, loaded };
  protected PopulateMenuState populateMenuState = PopulateMenuState.unloaded;

  protected static Color closeRemoveColor = new Color(0.847f, 0.32f, 0.30f);
  protected static Color defaultMenuColor = Color.blue / 2;
  protected static Color grayColor = new Color(1, 1, 1) / 2;
  protected static Color dangerColor = new Color(1, 0.5f, 0.5f) / 2;
  protected static Color okColor = Color.green / 2;
  protected static Color warningColor = Color.yellow / 2;

  public void Start()
  {
    cm = GetComponent<CircleMenu>();
  }

  public virtual void Update()
  {
    if (ControlerInput.instance.triggerLeft)
    {
      Close();
    }
  }

  public string Icon(string id)
  {
    return $" <font=\"FontAwesome\">{id}</font> ";
  }

  public void Close()
  {
    if (node != null) node.IsActiveInMenu = false;
    if (edge != null) edge.IsActiveInMenu = false;
    populateMenuState = PopulateMenuState.unloaded;
    limitSlider.SetActive(false);
    node = null;
    subMenu = "";
    edge = null;
    graph = null;
    if (cm != null)
    {
      cm.Close();
      controlerModel.SetActive(true);
    }
  }

  protected bool GraphHasSelectedVariable()
  {
    return graph.selection.Find((edge) => edge.IsVariable || edge.displayObject.IsVariable || edge.displaySubject.IsVariable) != null;
  }

  protected HashSet<string> SelectedVariableNames()
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

  public void PopulateGraphMenu()
  {
    cm.AddButton(Icon("\uF1E0") + "Layout: Force Directed 3D", Color.green / 2, () =>
    {
      graph.SetLayout(Graph.Layout.FruchtermanReingold);
    });

    cm.AddButton(Icon("\uF5EE") + "Layout: Force Directed 2D", Color.green / 2, () =>
    {
      graph.SetLayout(Graph.Layout.SpatialGrid2D);
    });

    cm.AddButton(Icon("\uF0E8") + "Layout: Hierarchical View", Color.green / 2, () =>
    {
      graph.SetLayout(Graph.Layout.HierarchicalView);
    });

    cm.AddButton(Icon("\uF126") + "Layout: Class Hierarchy", Color.green / 2, () =>
    {
      graph.SetLayout(Graph.Layout.ClassHierarchy);
    });

    cm.AddButton(Icon("\uF021") + "Auto layout", Color.yellow / 2, () =>
    {
      graph.layout.CalculateLayout();
    });

    cm.AddButton(Icon("\uF057") + "Close Graph", new Color(1, 0.5f, 0.5f) / 2, () =>
    {
      graph.Remove();
      Close();
    });


    cm.AddButton(Icon("\uF023") + "Pin all nodes", new Color(0.5f, 0.5f, 0.5f) / 2, () =>
    {
      graph.PinAllNodes(true);
    });

    cm.AddButton(Icon("\uF3C1") + "Unpin all nodes", new Color(0.5f, 0.5f, 0.5f) / 2, () =>
    {
      graph.PinAllNodes(false);
    });

    if (graph.boundingSphere.IsVisible())
    {
      cm.AddButton(Icon("\uF070") + "Hide sphere", new Color(0, 0.9f, 1.0f) / 2, () =>
      {
        graph.boundingSphere.Hide();
        subMenu = "Graph";
        cm.Close();
        PopulateGraphMenu();
        cm.ReBuild();
      });
    }
    else
    {
      cm.AddButton(Icon("\uF06E") + "Show sphere", new Color(0, 0.9f, 1.0f) / 2, () =>
      {
        graph.boundingSphere.Show();
        populateMenuState = PopulateMenuState.unloaded;
        cm.Close();
        PopulateGraphMenu();
        cm.ReBuild();
      });
    }

    cm.AddButton(Icon("\uF0C7") + "Save this Graph as ntriples", new Color(1, 0.5f, 0.5f) / 2, () =>
      {
        Graph graphToSave = graph;
        Utils.GetStringFromVRKeyboard((string fileName) =>
        {
          SaveLoadGraph.Save(graphToSave, fileName);
        }
        , "graph", "Enter a filename...");
        Close();
      });

    if (graph.subGraphs.Count > 0)
    {
      cm.AddButton(Icon("\uF057") + "Close all child graphs", new Color(1, 0.5f, 0.5f) / 2, () =>
      {
        graph.RemoveSubGraphs();
        Close();
      });
    }

    if (graph.parentGraph != null && graph.creationQuery != "")
    {
      cm.AddButton(Icon("\uF057") + "Close unmodified sibling graphs", new Color(1, 0.5f, 0.5f) / 2, () =>
      {
        graph.RemoveGraphsOfSameQuery();
        Close();
      });
    }
  }
}
