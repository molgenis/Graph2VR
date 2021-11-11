using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VDS.RDF;
using VDS.RDF.Query;

public class Node : MonoBehaviour
{
  private Canvas infoPanel;
  public bool pinned = false;
  public bool isSelected = false;
  public bool isPointerHovered = false;
  public bool isControllerHovered = false;
  public bool isControllerGrabbed = false;

  public string uri = ""; // Full URI, empty if literal
  public string label = "";
  private string cachedNodeLabel = ""; // label of the node, (before it gets converted to variable)
  private Color cachedNodeColor; // color of the node, (before it gets converted to variable)

  public INode graphNode;
  public Color defaultColor;

  private TMPro.TextMeshPro textMesh;
  // Variables for the Force-directed algorithm
  public Vector3 displacement;

  public bool isVariable { get; private set; } = false;

  public void Awake()
  {
    textMesh = GetComponentInChildren<TMPro.TextMeshPro>(true);
  }

  public void Start()
  {
    InvokeRepeating("UpdateDisplay", 1, 1);
    RefineGraph();
  }

  void Update()
  {
    if (isControllerHovered || isPointerHovered)
    {
      SetColor(hoverColor);
    }
    else if (isControllerGrabbed)
    {
      SetColor(grabbedColor);
    }
    else if (isSelected)
    {
      SetColor(selectedColor);
    }
    else
    {
      SetColor(defaultColor);
    }


    transform.rotation = Quaternion.LookRotation(Camera.main.transform.position - transform.position, Vector3.up);
    if (isControllerGrabbed || isPointerHovered)
    {
      textMesh.transform.localScale = Vector3.one * 0.6f;
    }
    else
    {
      textMesh.transform.localScale = Vector3.one * 0.3f;
    }
  }
  public void Select()
  {
    isSelected = true;
    transform.Find("Selected").gameObject.SetActive(true);
    transform.Find("Selected").gameObject.GetComponent<Renderer>().material.SetColor("_Color", Graph.instance.edgeSelectedColor);

  }

  public void Deselect()
  {
    isSelected = false;
    transform.Find("Selected").gameObject.SetActive(false);
  }

  public void RefineGraph()
  {
    if (graphNode == null)
    {
      return;
    }
    else
    {
      ConnectLabelToNode();
    }
  }

  private void ConnectLabelToNode()
  {
    foreach (Triple tripleWithSubject in graphNode.Graph.GetTriplesWithSubject(graphNode))
    {
      if (IsLabelPredicate(tripleWithSubject.Predicate))
      {
        SetLabel(tripleWithSubject.Object.ToString());
        Graph.instance.Remove(Graph.instance.GetByINode(tripleWithSubject.Object));
        break;
      }
    }
  }

  private boolean IsLabelPredicate(INode predicate)
  {
    return predicate.ToString() == "http://www.w3.org/2000/01/rdf-schema#label";
  }

  public void MakeVariable()
  {
    isVariable = true;
    cachedNodeColor = defaultColor;
    SetDefaultColor(Graph.instance.variableNodeColor);

    string newLabel = Graph.instance.variableNameManager.GetVariableName(uri);
    SetLabel(newLabel);
  }

  public void UndoConversion()
  {
    isVariable = false;
    SetDefaultColor(cachedNodeColor);
    SetLabel(cachedNodeLabel);
  }

  public void SetDefaultColor(Color color)
  {
    defaultColor = color;
    SetColor(color);
  }

  public void SetColor(Color color)
  {
    GetComponent<Renderer>().material.color = color;
  }

  public void SetLabel(string label)
  {
    if (isVariable)
    {
      this.label = GetVariableLabel();
    }
    else
    {
      this.label = label.Replace("@" + Main.instance.languageCode, "");
      cachedNodeLabel = this.label;
    }
    UpdateDisplay();
  }

  private string SetVariableLabel(string label)
  {
    return getVariableLabelPrefix() + label.Replace("@" + Main.instance.languageCode, "");
  }

  private string getVariableLabelPrefix(string label)
  {
    return label.StartsWith("?") ? "" : "?";
  }

  public string GetLabel()
  {
    return this.label;
  }

  public string GetURIAsString()
  {
    return this.uri;
  }

  public void SetURI(string uri)
  {
    this.uri = uri;
  }

  public System.Uri GetURI()
  {
    return VDS.RDF.UriFactory.Create(this.uri);
  }

  public string GetQueryLabel()
  {
    if (isVariable)
    {
      return GetLabel();
    }
    else
    {
      return GetURIAsString();
    }
  }

  private void UpdateDisplay()
  {
    textMesh.text = (label == "") ? uri : label;
  }

  public void ToggleInfoPanel()
  {
    if (infoPanel == null)
    {
      InitiateInfoPanel();
    }
    else
    {
      infoPanel.enabled = !infoPanel.enabled;
    }
    PositionInfoPanel();
  }

  private void InitiateInfoPanel()
  {
    infoPanel = Instantiate<Canvas>(Resources.Load<Canvas>("UI/ContextMenu"));
    infoPanel.renderMode = RenderMode.WorldSpace;
    infoPanel.worldCamera = GameObject.Find("Controller (right)").GetComponent<Camera>();
    ContextMenuHandler selectorHandler = infoPanel.GetComponent<ContextMenuHandler>();
    selectorHandler.Initiate(this);
  }

  private void PositionInfoPanel()
  {
    infoPanel.transform.position = transform.position;
    infoPanel.transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position, Vector3.up);
    infoPanel.transform.position += infoPanel.transform.rotation * new Vector3(1.0f, 0, 0) * Mathf.Max(transform.lossyScale.x, gameObject.transform.lossyScale.y);
  }
}
