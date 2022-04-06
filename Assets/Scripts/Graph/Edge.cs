using UnityEngine;
using VDS.RDF;

public class Edge : MonoBehaviour
{
  public string uri = "";
  public Graph graph;
  public Node displaySubject;
  public Node displayObject;
  public INode graphSubject;
  public INode graphPredicate;
  public INode graphObject;

  private Transform arrow;
  private new CapsuleCollider collider;
  private LineRenderer lineRenderer;
  private TMPro.TextMeshPro textFront;
  private TMPro.TextMeshPro textBack;

  private string textShort = "";
  private string textLong = "";
  public string variableName = "";

  // refactor: don't cache but get the correct color based on the type
  private Color cachedNodeColor; // color of the node, (before it gets converted to variable)

  private NodeFactory nodeFactory = new NodeFactory();

  private bool isVariable = false;
  private bool isSelected = false;
  private bool isPointerHovered = false;
  private bool isControllerHovered = false;
  private bool isControllerGrabbed = false;
  private bool isSubclassOfRelation = false;

  public bool IsVariable
  {
    get => isVariable;
    set
    {
      isVariable = value;
      UpdateColor();
      UpdateEdgeText();
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
      UpdateEdgeText();
    }
  }

  public bool IsControllerHovered
  {
    get => isControllerHovered;
    set
    {
      isControllerHovered = value;
      UpdateColor();
      UpdateEdgeText();
    }
  }

  public bool IsControllerGrabbed
  {
    get => isControllerGrabbed;
    set
    {
      isControllerGrabbed = value;
      UpdateColor();
      UpdateEdgeText();
    }
  }

  public bool IsSubclassOfRelation
  {
    get => uri.Equals("http://www.w3.org/2000/01/rdf-schema#subClassOf", System.StringComparison.OrdinalIgnoreCase);
    set
    {
      isSubclassOfRelation = uri.Equals("http://www.w3.org/2000/01/rdf-schema#subClassOf", System.StringComparison.OrdinalIgnoreCase);
      UpdateColor();
    }
  }

  private void Awake()
  {
    lineRenderer = GetComponent<LineRenderer>();
    lineRenderer.useWorldSpace = false;
    arrow = transform.Find("Arrow");
    collider = transform.Find("Collider").GetComponent<CapsuleCollider>();
  }

  private void Start()
  {
    InitializeTexts();
    UpdatePosition();
    UpdateColor();
  }

  private void InitializeTexts()
  {
    textFront = transform.Find("FrontText").GetComponent<TMPro.TextMeshPro>();

    string shortName = graph.GetShortName(uri);
    if (shortName != "")
    {
      textShort = textLong = shortName;
    }
    else
    {
      textShort = Utils.GetShortLabelFromUri(uri);
      textLong = uri;
    }
    UpdateEdgeText();
  }
  private void UpdateColor()
  {
    if (IsControllerHovered || IsPointerHovered)
    {
      SetColor(ColorSettings.instance.edgeHoverColor);
    }
    else if (IsControllerGrabbed)
    {
      SetColor(ColorSettings.instance.edgeGrabbedColor);
    }
    else if (IsSelected)
    {
      SetColor(ColorSettings.instance.edgeSelectedColor);
    }
    else if (IsVariable)
    {
      SetColor(ColorSettings.instance.variableColor);
    }
    else if (IsSubclassOfRelation)
    {
      SetColor(ColorSettings.instance.defaultEdgeColor, ColorSettings.instance.arrowheadSubclassOfColor);
    }
    else
    {
      SetColor(ColorSettings.instance.defaultEdgeColor);
    }
  }

  private void Update()
  {
    if (displaySubject == null || displayObject == null)
    {
      return;
    }
    UpdatePosition();
  }

  public void Select()
  {
    IsSelected = true;
    displaySubject.Select();
    displayObject.Select();

    if (graphSubject != null && graphObject != null)
    {
      graph.AddToSelection(this);
    }
  }

  public void Deselect()
  {
    IsSelected = false;
    displaySubject.Deselect();
    displayObject.Deselect();

    if (graphSubject != null && graphObject != null)
    {
      graph.RemoveFromSelection(this);
    }
  }

  public void MakeVariable()
  {
    IsVariable = true;
    graphPredicate = GetVariableInode();
  }

  private IVariableNode GetVariableInode()
  {
    SetVariableName(graph.variableNameManager.GetVariableName(graphPredicate));
    return nodeFactory.CreateVariableNode(variableName);
  }

  public void UndoConversion()
  {
    IsVariable = false;
    variableName = "";
  }

  public string GetQueryString()
  {
    return displaySubject.GetQueryLabel() + " " + GetQueryLabel() + " " + displayObject.GetQueryLabel() + " .\n";
  }

  public string GetQueryLabel()
  {
    if (IsVariable)
    {
      return variableName;
    }
    else
    {
      return "<" + uri + ">";
    }
  }

  public bool Equals(INode Subject, INode Predicate, INode Object)
  {
    return Subject.Equals(graphSubject) && Predicate.Equals(graphPredicate) && Object.Equals(graphObject);
  }

  public void SetColor(Color color)
  {
    lineRenderer.material.color = color;
    arrow.GetComponent<Renderer>().material.color = color;
  }

  public void SetColor(Color lineRenderercolor, Color Arrowheadcolor)
  {
    lineRenderer.material.color = lineRenderercolor;
    arrow.GetComponent<Renderer>().material.color = Arrowheadcolor;
  }

  private void UpdatePosition()
  {
    transform.position = (displaySubject.transform.position + displayObject.transform.position) * 0.5f;
    Vector3 fromPosition = displaySubject.transform.position - transform.position;
    Vector3 toPosition = displayObject.transform.position - transform.position;
    float distance = ((toPosition - fromPosition).magnitude);
    Vector3 normal = (toPosition - fromPosition).normalized;
    Vector2 textRotation = CalculateAngles(fromPosition, toPosition);

    UpdateLineRenderer(fromPosition, toPosition, distance, normal);
    UpdateArrow(fromPosition, toPosition, normal);
    PositionCollider(textRotation, distance);
    RotateText(textRotation);
    UpdateTextSize(distance);
  }

  private void UpdateArrow(Vector3 fromPosition, Vector3 toPosition, Vector3 normal)
  {
    arrow.localPosition = (transform.worldToLocalMatrix * (toPosition - (normal * (displayObject.transform.lossyScale.x * 0.5f))));
    arrow.rotation = Quaternion.FromToRotation(Vector3.up, normal);

  }

  private void UpdateLineRenderer(Vector3 fromPosition, Vector3 toPosition, float distance, Vector3 normal)
  {
    lineRenderer.startWidth = lineRenderer.endWidth = 0.005f * transform.lossyScale.magnitude;
    lineRenderer.SetPosition(0, transform.worldToLocalMatrix * (fromPosition + normal * (displaySubject.transform.lossyScale.x * 0.5f)));
    lineRenderer.SetPosition(1, transform.worldToLocalMatrix * (toPosition - (normal * ((displayObject.transform.lossyScale.x * 0.5f) + (arrow.lossyScale.x * 0.05f)))));

  }

  private void PositionCollider(Vector2 backRotation, float distance)
  {
    collider.transform.rotation = Quaternion.Euler(0, backRotation.x, backRotation.y);
    collider.transform.localPosition = Vector3.zero;
    collider.height = distance * 0.85f;
  }

  private void RotateText(Vector2 frontRotation)
  {
    textFront.transform.rotation = Quaternion.Euler(0, frontRotation.x, frontRotation.y); // note this is world rotation
    textFront.transform.localPosition = textFront.transform.localRotation * (Vector3.up * 0.025f); // note this is local position
                                                                                                   // Rotate the text to face the camera
    if (Vector3.Dot(textFront.transform.forward, Camera.main.transform.forward) < 0)
    {
      textFront.transform.localRotation *= Quaternion.AngleAxis(180, Vector3.up);
    }

  }

  private void UpdateTextSize(float distance)
  {
    float textDistance = (distance * (1 / textFront.transform.localScale.x)) * 0.8f;

    // only scale text every 60 frames for performance reasons
    if (GetInstanceID() % 60 == Time.frameCount % 60)
    {
      textFront.rectTransform.sizeDelta = new Vector2(textDistance, 1);
    }
  }

  private void UpdateEdgeText()
  {
    if (IsVariable)
    {
      textFront.text = variableName;
    }
    else if (IsPointerHovered || IsControllerHovered || IsControllerGrabbed)
    {
      textFront.text = textLong;
    }
    else
    {
      textFront.text = textShort;
    }
  }

  public void SetVariableName(string variableName)
  {
    this.variableName = variableName;
    UpdateEdgeText();
  }

  private Vector2 CalculateAngles(Vector3 fromPosition, Vector3 toPosition)
  {
    if (Vector3.Distance(fromPosition, toPosition) == 0)
    {
      return Vector2.zero;
    }
    float height = (toPosition.y - fromPosition.y);
    float angle = 90;
    float yRotation = angle + Mathf.Atan2(fromPosition.x, fromPosition.z) * (180 / Mathf.PI);
    float zRotation = Mathf.Asin(height / Vector3.Distance(fromPosition, toPosition)) * (180 / Mathf.PI);
    return new Vector2(yRotation, zRotation);
  }
}
