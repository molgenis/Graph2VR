using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dweiss;
using UnityEngine;
using UnityEngine.Networking;
using VDS.RDF;

public class Node : MonoBehaviour
{
  public Graph graph;
  private Canvas infoPanel;
  private bool isVariable = false;
  private bool isSelected = false;
  private bool isActiveInMenu = false;
  private bool isPointerHovered = false;
  private bool isControllerHovered = false;
  private bool isControllerGrabbed = false;

  public List<Edge> connections = new List<Edge>();
  public void AddConnection(Edge edge)
  {
    if (!connections.Contains(edge)) connections.Add(edge);
  }

  public bool IsActiveInMenu
  {
    get => isActiveInMenu;
    set
    {
      isActiveInMenu = value;
      UpdateColor();
    }
  }

  public bool IsVariable
  {
    get => isVariable;
    set
    {
      isVariable = value;
      UpdateColor();
    }
  }

  public bool IsSelected
  {
    get => isSelected;
    set
    {
      isSelected = value;
      UpdateColor();
    }
  }

  public bool IsPointerHovered
  {
    get => isPointerHovered;
    set
    {
      isPointerHovered = value;
      UpdateColor();
    }
  }

  public bool IsControllerHovered
  {
    get => isControllerHovered;
    set
    {
      isControllerHovered = value;
      UpdateColor();
    }
  }

  public bool IsControllerGrabbed
  {
    get => isControllerGrabbed;
    set
    {
      isControllerGrabbed = value;
      UpdateColor();
    }
  }

  public string uri = ""; // Full URI, empty if literal
  public string label = "";
  private string cachedNodeLabel = ""; // label of the node, (before it gets converted to variable)

  public INode graphNode;

  private TMPro.TextMeshPro textMesh;
  // Variables for the Force-directed algorithm
  public Vector3 displacement;

  public void Awake()
  {
    textMesh = GetComponentInChildren<TMPro.TextMeshPro>(true);
  }

  public void Start()
  {
    InvokeRepeating("UpdateDisplay", 1, 1);
    RefineGraph();
    UpdateColor();
  }

  private void UpdateColor()
  {
    if (IsControllerHovered || IsPointerHovered)
    {
      SetColor(ColorSettings.instance.edgeHoverColor);
    }
    else if (IsActiveInMenu)
    {
      SetColor(ColorSettings.instance.edgeGrabbedColor);
    }
    else if (IsControllerGrabbed)
    {
      SetColor(ColorSettings.instance.edgeGrabbedColor);
    }
    else if (IsVariable)
    {
      SetColor(ColorSettings.instance.variableColor);
    }
    else if (graphNode != null)
    {
      UpdateColorByNodeType();
    }
    else
    {
      SetColor(ColorSettings.instance.defaultNodeColor);
    }
  }

  public bool UpdateColorByVOWL()
  {
    List<Edge> nodeTypes = graph.edgeList.FindAll(
      edge => edge.displaySubject == this &&
      edge.uri == "http://www.w3.org/1999/02/22-rdf-syntax-ns#type"
    );

    foreach (Edge edge in nodeTypes)
    {
      switch (edge.displayObject.uri)
      {
        case "http://www.w3.org/2002/07/owl#Thing":
          SetColor(ColorSettings.instance.nodeOwlClassColor);
          return true;
        case "http://www.w3.org/2002/07/owl#Class":
          SetColor(ColorSettings.instance.nodeOwlClassColor);
          return true;
        case "https://www.w3.org/1999/02/22-rdf-syntax-ns#subClassOf":
          SetColor(ColorSettings.instance.arrowheadSubclassOfColor);
          return true;
        case "https://www.w3.org/1999/02/22-rdf-syntax-ns#Property":
          SetColor(ColorSettings.instance.nodeRdfsClassColor);
          return true;
      }
    }
    return false;
  }

  private void UpdateColorByNodeType()
  {
    // First check VOWL color schema
    if (!UpdateColorByVOWL())
    {
      // If we didn't find a color in VOWL schema check the node type
      switch (graphNode.NodeType)
      {
        case NodeType.Variable:
          SetColor(ColorSettings.instance.variableColor);
          break;
        case NodeType.Blank:
          uri = "";
          SetColor(ColorSettings.instance.blankNodeColor);
          break;
        case NodeType.Literal:
          SetLabel(((ILiteralNode)graphNode).Value);
          uri = "";
          SetColor(ColorSettings.instance.literalColor);
          break;
        case NodeType.Uri:
          uri = ((IUriNode)graphNode).Uri.ToString();
          SetColor(ColorSettings.instance.uriColor);
          break;
      }
    }
  }

  void Update()
  {

    transform.rotation = Quaternion.LookRotation(Camera.main.transform.position - transform.position, Vector3.up);
    if (isControllerGrabbed || isPointerHovered)
    {
      textMesh.transform.localScale = Vector3.one * 0.6f;
    }
    else
    {
      textMesh.transform.localScale = Vector3.one * 0.3f;
    }

    // Clamp position
    if (transform.position.y < 0)
    {
      transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }

  }
  public void Select()
  {
    isSelected = true;
    transform.Find("Selected").gameObject.SetActive(true);
    transform.Find("Selected").gameObject.GetComponentInChildren<Renderer>().material.SetColor("_EmissionColor", ColorSettings.instance.nodeSelectedColor);
    UpdateColor();
  }

  public void Deselect()
  {
    isSelected = false;
    transform.Find("Selected").gameObject.SetActive(false);
    UpdateColor();
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
      ConnectImageToNode();
    }
  }

  private void ConnectLabelToNode()
  {
    Edge labelEdge = graph.edgeList.Find(edge => edge.displaySubject == this && IsLabelPredicate(edge.uri));
    if (labelEdge != null)
    {
      SetLabel(labelEdge.displayObject.uri);
      graph.RemoveNode(labelEdge.displayObject);
    }
  }

  private bool IsLabelPredicate(string predicate)
  {
    return predicate == "http://www.w3.org/2000/01/rdf-schema#label";
  }

  private void ConnectImageToNode()
  {
    List<Edge> imageEdge = graph.edgeList.FindAll(edge => edge.displaySubject);
    foreach (Edge edge in imageEdge)
    {
      if (IsImagePredicate(edge.uri))
      {
        // Todo: we might want to modify the mesh when we are certain that the texture exists and is not a 404 url
        Mesh m = new Mesh();
        m.vertices = new Vector3[]{
          new Vector3(-1, -1.5f, 0),
          new Vector3(1, -1.5f, 0),
          new Vector3(1, 0.5f, 0),
          new Vector3(-1, 0.5f, 0)
        };

        m.uv = new Vector2[]{
          new Vector2(0, 0),
          new Vector2(0, 1),
          new Vector2(1, 1),
          new Vector2(1, 0),
        };
        m.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        m.RecalculateBounds();
        m.RecalculateNormals();
        MeshFilter filter = GetComponent<MeshFilter>();
        filter.mesh = m;

        StartCoroutine(FetchTexture(edge.displayObject.uri));
        graph.RemoveNode(edge.displayObject);
        // Do not break, node can have multiple images, some of them can be 404 url's so lets go through all of them
        //break;
      }
    }
  }

  private bool IsImagePredicate(string predicate)
  {
    foreach (string pred in Settings.Instance.ImagePredicates)
    {
      if (predicate.Equals(pred))
      {
        return true;
      }
    }
    return false;
  }

  public void MakeVariable()
  {
    isVariable = true;

    string newLabel = graph.variableNameManager.GetVariableName(graphNode);
    SetLabel(newLabel);
    UpdateColor();
  }

  public void UndoConversion()
  {
    isVariable = false;
    SetLabel(cachedNodeLabel);
    UpdateColor();
  }

  public void SetColor(Color color)
  {
    GetComponent<Renderer>().material.color = color;
  }

  public IEnumerator FetchTexture(string url)
  {
    UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
    yield return www.SendWebRequest();

    if (www.result != UnityWebRequest.Result.Success)
    {
      Debug.Log(www.error);
    }
    else
    {
      GetComponent<Renderer>().material.mainTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;

      // handle aspect ratio
      float scale = 0.1f;
      float width = 2.0f * scale;
      float aspect = (float)((DownloadHandlerTexture)www.downloadHandler).texture.width / ((DownloadHandlerTexture)www.downloadHandler).texture.height;
      float height = width / aspect;
      GetComponent<Renderer>().gameObject.transform.localScale = new Vector3(width, height, scale);
    }
  }

  public void SetLabel(string label)
  {
    if (isVariable)
    {
      this.label = GetVariableLabel(label);
    }
    else
    {
      this.label = label.Replace("@" + Main.instance.languageCode, "");
      cachedNodeLabel = this.label;
    }
    UpdateDisplay();
  }

  private string GetVariableLabel(string label)
  {
    return GetVariableLabelPrefix(label) + label.Replace("@" + Main.instance.languageCode, "");
  }

  private string GetVariableLabelPrefix(string label)
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
      return graph.RealNodeValue(graphNode);
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
