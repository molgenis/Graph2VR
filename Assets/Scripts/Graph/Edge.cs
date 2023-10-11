﻿using Dweiss;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using VDS.RDF;
using VDS.RDF.Query;

public class Edge : MonoBehaviour
{
  public string uri = "";
  public Graph graph;
  public Node displaySubject;
  public Node displayObject;
  public INode graphSubject;
  public INode graphPredicate;
  public INode graphObject;
  private INode nonVariableGraphPredicate;
  private Transform arrow;
  private CapsuleCollider directLineCollider;
  private GameObject bendLineColliders;
  private LineRenderer lineRenderer;
  public TMPro.TextMeshPro textFront;

  // 0 its not a optional value
  // > 0 the unique optional index of the triple
  public long optionalTripleCounter = 0;
  public static long optionalCounter = 0;

  public string textShort = "";
  private string textLong = "";
  public string variableName = "";
  public enum LineType { Direct, Bend, Circle }
  public LineType lineType = LineType.Direct;

  private Vector3 bendDirectionVector = Vector3.zero;
  public float bendDirectionOffset = 0;
  public bool flippedDirection = false;

  private readonly NodeFactory nodeFactory = new();

  public bool isOptional = false;
  private bool isVariable = false;
  private bool isSelected = false;
  private bool isActiveInMenu = false;
  private bool isPointerHovered = false;
  private bool isControllerHovered = false;
  private bool isControllerGrabbed = false;
  private Stopwatch throttle;

  private static readonly int bendResolution = 10;
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

  public bool IsOptional
  {
    get => isOptional;
    set
    {
      isOptional = value;
      optionalCounter++;
      optionalTripleCounter = optionalCounter;
      if (isOptional)
      {
        lineRenderer.material.mainTexture = Settings.Instance.lineDashed;
        lineRenderer.material.SetFloat("_AlphaClip", 1);
        lineRenderer.material.SetFloat("_Cutoff", 0.5f);
        lineRenderer.material.EnableKeyword("_ALPHATEST_ON");
      }
      else
      {
        lineRenderer.material.mainTexture = Settings.Instance.line;
        lineRenderer.material.DisableKeyword("_ALPHATEST_ON");
      }
    }
  }


  public bool IsSelected
  {
    get => isSelected;
    set
    {
      isSelected = value;
      UpdateColor();
      displayObject.UpdateSelectionStatus();
      displaySubject.UpdateSelectionStatus();
      if (isSelected == false)
      {
        IsOptional = false;
      }
    }
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
      UpdateColor();
    }
  }

  private void Awake()
  {
    lineRenderer = GetComponent<LineRenderer>();
    lineRenderer.useWorldSpace = false;
    arrow = transform.Find("Arrow");
    directLineCollider = transform.Find("Collider").GetComponent<CapsuleCollider>();
    bendDirectionVector = Random.insideUnitSphere * 0.5f;
    bendLineColliders = new GameObject("Bend Line Colliders");
    bendLineColliders.transform.SetParent(transform);
    bendLineColliders.transform.localPosition = Vector3.zero;
    bendLineColliders.transform.localRotation = Quaternion.identity;
    bendLineColliders.transform.localScale = Vector3.one;
    throttle = new Stopwatch();
    throttle.Start();
  }

  private void Start()
  {
    InitializeTexts();
    UpdateEdgeDisplay();
    UpdateColor();
  }

  public void InitializeTexts()
  {
    string shortName = graph.GetShortName(uri);
    if (shortName != "")
    {
      textShort = shortName;
    }
    else
    {
      textShort = Utils.GetShortLabelFromUri(uri);
    }
    textLong = uri;
    UpdateEdgeText();
    QueryService.Instance.GetLabelForPredicate(uri, (SparqlResultSet resultSet, object state) =>
    {
      if (resultSet != null && resultSet.Count > 0 && resultSet[0].HasValue("label"))
      {
        shortName = resultSet[0]["label"].ToString();
      }
      if (shortName != uri && shortName != "" && shortName != null)
      {
        textShort = shortName;
      }
      UpdateEdgeText();
    });
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
      SetColor(ColorSettings.instance.defaultEdgeColor, Settings.Instance.arrowheadSubclassOfColor);
    }
    else
    {
      SetColor(ColorSettings.instance.defaultEdgeColor);
    }
  }

  private Vector3 oldDisplayObjectPosition = Vector3.zero;
  private Vector3 oldDisplaySubjectPosition = Vector3.zero;
  private Vector3 oldCameraPosition = Vector3.zero;
  private Quaternion oldCameraRotation = Quaternion.identity;
  private void Update()
  {
    if (displaySubject == null || displayObject == null)
    {
      return;
    }

    // Update Edge if it has moved
    if (displayObject.transform.position != oldDisplayObjectPosition || displaySubject.transform.position != oldDisplaySubjectPosition)
    {
      UpdateEdgeDisplay();
      oldDisplayObjectPosition = displayObject.transform.position;
      oldDisplaySubjectPosition = displaySubject.transform.position;
    }

    // Update if camera has moved
    if (Camera.main.transform.position != oldCameraPosition || Camera.main.transform.rotation != oldCameraRotation)
    {
      UpdateEdgeTextRotations();

      if (lineType == LineType.Circle)
      {
        textFront.transform.rotation = Quaternion.LookRotation(textFront.transform.position - Camera.main.transform.position, Vector3.up);
      }
      oldCameraPosition = Camera.main.transform.position;
      oldCameraRotation = Camera.main.transform.rotation;
    }
  }

  public void Select()
  {
    if (!IsSelected)
    {
      IsSelected = true;
      graph.AddToSelection(this);
    }
  }

  public void Deselect()
  {
    if (IsSelected)
    {
      IsSelected = false;
      graph.RemoveFromSelection(this);
    }
  }

  public void MakeVariable()
  {
    IsVariable = true;
    nonVariableGraphPredicate = graphPredicate;
    graphPredicate = GetVariableInode();
  }

  private IVariableNode GetVariableInode()
  {
    SetVariableName(graph.variableNameManager.GetVariableName(graphPredicate));
    return nodeFactory.CreateVariableNode(variableName);
  }

  public void UndoConversion()
  {
    graphPredicate = nonVariableGraphPredicate;
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

  private void UpdateEdgeTextRotations()
  {
    Vector3 fromPosition = displaySubject.transform.position - transform.position;
    Vector3 toPosition = displayObject.transform.position - transform.position;
    Vector2 textRotation = CalculateAngles(fromPosition, toPosition);

    Vector3 localBendCenter = lineRenderer.GetPosition((int)(lineRenderer.positionCount * 0.5f));
    if (lineType == LineType.Circle)
    {
      textFront.transform.rotation = Quaternion.identity;
      textFront.transform.localPosition = localBendCenter + (bendDirectionVector.normalized * 0.025f);
    }
    else
    {
      textFront.transform.rotation = Quaternion.Euler(0, textRotation.x, textRotation.y); // note this is world rotation
      if (lineType == LineType.Direct)
      {
        textFront.transform.localPosition = textFront.transform.localRotation * (Vector3.up * 0.025f); // note this is local position
      }
      else
      {
        textFront.transform.localPosition = localBendCenter + (textFront.transform.localRotation * (Vector3.up * 0.025f)); // note this is local position
      }

      if (Vector3.Dot(textFront.transform.forward, Camera.main.transform.forward) < 0)
      {
        textFront.transform.localRotation *= Quaternion.AngleAxis(180, Vector3.up);
      }
    }
  }

  public void Remove()
  {
    displayObject.connections.Remove(this);
    displaySubject.connections.Remove(this);
    UpdateEdgeLines(true);
    displayObject.UpdateSelectionStatus();
    displaySubject.UpdateSelectionStatus();
    if (IsSelected)
    {
      IsSelected = false;
      graph.RemoveFromSelection(this);
    }
  }

  public void UpdateEdgeLines(bool removeSelf = false)
  {
    // Check if edge overlaps with an other
    List<Edge> foundCollisions = new();
    foreach (Edge edgeToCheck in graph.edgeList)
    {
      if ((displaySubject.uri == edgeToCheck.displaySubject.uri && displayObject.uri == edgeToCheck.displayObject.uri))
      {
        foundCollisions.Add(edgeToCheck);
        edgeToCheck.flippedDirection = false;
      }
      else if (displayObject.uri == edgeToCheck.displaySubject.uri && displaySubject.uri == edgeToCheck.displayObject.uri)
      {
        foundCollisions.Add(edgeToCheck);
        edgeToCheck.flippedDirection = true;
      }
    }

    int collisionCount = (foundCollisions.Count - (removeSelf ? 1 : 0));
    if (collisionCount > 0)
    {
      int index = 0;
      if (!removeSelf) foundCollisions.Add(this);
      foreach (Edge foundEdge in foundCollisions)
      {
        foundEdge.lineType = Edge.LineType.Bend;
        foundEdge.bendDirectionOffset = (360f / (foundCollisions.Count)) * index;
        foundEdge.UpdateEdgeDisplay();
        index++;
      }
    }
    else if (removeSelf)
    {
      foreach (Edge foundEdge in foundCollisions)
      {
        foundEdge.lineType = Edge.LineType.Direct;
        foundEdge.UpdateEdgeDisplay();
      }
    }
  }

  public void UpdateEdgeDisplay()
  {
    transform.position = (displaySubject.transform.position + displayObject.transform.position) * 0.5f;
    Vector3 fromPosition = displaySubject.transform.position - transform.position;
    Vector3 toPosition = displayObject.transform.position - transform.position;
    float distance = ((toPosition - fromPosition).magnitude);
    Vector3 normal = (toPosition - fromPosition).normalized;
    Vector2 textRotation = CalculateAngles(fromPosition, toPosition);

    UpdateBendDirectionVector(normal);
    UpdateLineRenderer(fromPosition, toPosition, normal);
    UpdateColliders(textRotation, distance);
    UpdateTextSize(distance);
  }

  private void UpdateBendDirectionVector(Vector3 normal)
  {
    if (flippedDirection)
    {
      bendDirectionVector = Quaternion.FromToRotation(transform.up, normal) * Quaternion.Euler(90, 0, bendDirectionOffset) * Vector3.up * 0.2f;
    }
    else
    {
      bendDirectionVector = Quaternion.FromToRotation(-transform.up, normal) * Quaternion.Euler(90, 0, bendDirectionOffset + 180) * Vector3.down * 0.2f;
    }
  }

  private void UpdateArrow(Vector3 toPosition, Vector3 normal)
  {
    arrow.gameObject.SetActive(true);
    arrow.localPosition = (transform.worldToLocalMatrix * (toPosition - (normal * (displayObject.transform.lossyScale.x * 0.5f))));
    arrow.rotation = Quaternion.FromToRotation(Vector3.up, normal);
  }

  private void UpdateLineRenderer(Vector3 fromPosition, Vector3 toPosition, Vector3 normal)
  {
    lineRenderer.startWidth = lineRenderer.endWidth = 0.003f * transform.lossyScale.magnitude;

    Vector3 from = transform.worldToLocalMatrix * (fromPosition + normal * (displaySubject.transform.lossyScale.x * 0.5f));
    Vector3 to = transform.worldToLocalMatrix * (toPosition - (normal * ((displayObject.transform.lossyScale.x * 0.5f) + (arrow.lossyScale.x * 0.05f))));

    if (lineType == LineType.Direct)
    {
      lineRenderer.positionCount = 2;
      lineRenderer.SetPosition(0, from);
      lineRenderer.SetPosition(1, to);
      UpdateArrow(toPosition, normal);
    }
    else if (lineType == LineType.Bend)
    {
      from = transform.worldToLocalMatrix * fromPosition;
      to = transform.worldToLocalMatrix * (toPosition - (normal * ((displayObject.transform.lossyScale.x * 0.5f) + (arrow.lossyScale.x * 0.05f))));
      lineRenderer.positionCount = bendResolution;
      for (int i = 0; i < lineRenderer.positionCount; i++)
      {
        float fraction = (float)i / (lineRenderer.positionCount - 1);
        Vector3 target = Vector3.Lerp(from, to, fraction);
        lineRenderer.SetPosition(i, target + (bendDirectionVector * Mathf.Sin(fraction * Mathf.PI)));
      }
      arrow.gameObject.SetActive(true);
      Vector3 arrowNormal = (lineRenderer.GetPosition(bendResolution - 1) - lineRenderer.GetPosition(bendResolution - 2)).normalized;
      arrow.localPosition = lineRenderer.GetPosition(bendResolution - 1) + (arrowNormal * (arrow.lossyScale.x * 0.05f));
      arrow.rotation = Quaternion.FromToRotation(Vector3.up, arrowNormal);
    }
    else
    {
      from = transform.worldToLocalMatrix * fromPosition;
      lineRenderer.positionCount = bendResolution;
      for (int i = 0; i < lineRenderer.positionCount; i++)
      {
        float fraction = (float)i / (lineRenderer.positionCount - 1);
        Vector3 sideNormal = Quaternion.FromToRotation(transform.up, bendDirectionVector.normalized) * Quaternion.Euler(90, 0, 0) * Vector3.up;
        Vector3 p1 = bendDirectionVector + (sideNormal * bendDirectionVector.magnitude);
        Vector3 p2 = bendDirectionVector + (-sideNormal * bendDirectionVector.magnitude);
        lineRenderer.SetPosition(i,
          Utils.CalculateCubicBezierPoint(fraction, from, from + p1, from + p2, from)
          );
      }
      arrow.gameObject.SetActive(false);
    }
  }

  private void UpdateColliders(Vector2 backRotation, float distance)
  {
    if (lineType == LineType.Direct)
    {
      directLineCollider.gameObject.SetActive(true);
      bendLineColliders.SetActive(false);
      directLineCollider.transform.rotation = Quaternion.Euler(0, backRotation.x, backRotation.y);
      directLineCollider.transform.localPosition = Vector3.zero;
      float parentScale = graph.transform.localScale.magnitude;
      directLineCollider.height = distance / parentScale;
    }
    else
    {
      directLineCollider.gameObject.SetActive(false);
      bendLineColliders.SetActive(true);
      foreach (Transform child in bendLineColliders.transform)
      {
        Destroy(child.gameObject);
      }

      for (int i = 1; i < lineRenderer.positionCount - 2; i++)
      {
        GameObject collider = new("Collider: (" + i + " to " + (i + 1) + ")");
        Vector3 from = lineRenderer.GetPosition(i);
        Vector3 to = lineRenderer.GetPosition(i + 1);
        Vector3 normal = (to - from).normalized;
        float size = Vector3.Distance(from, to);

        collider.transform.SetParent(bendLineColliders.transform);
        collider.transform.localPosition = (from + to) * 0.5f;
        collider.transform.localRotation = Quaternion.FromToRotation(Vector3.up, normal);
        collider.transform.localScale = Vector3.one;
        CapsuleCollider capsule = collider.AddComponent<CapsuleCollider>();
        capsule.radius = 0.015f;
        capsule.height = size + (capsule.radius * 2);
      }
    }
  }

  private void UpdateTextSize(float distance)
  {
    if (throttle.Elapsed.TotalSeconds > 1)
    {
      if (lineType != LineType.Circle)
      {
        //TODO: need a throttle function that also fires on the last update
        float textDistance = (distance * (1 / textFront.transform.localScale.x)) * 0.8f;
        textFront.rectTransform.sizeDelta = new Vector2(textDistance, 1);
        throttle.Reset();
        throttle.Start();
      }
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
