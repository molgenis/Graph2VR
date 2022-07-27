using System.Collections.Generic;
using UnityEngine;

public class ClassHierarchy : BaseLayoutAlgorithm
{
  private static readonly string SUBCLASS_OF_PREDICATE = "http://www.w3.org/2000/01/rdf-schema#subclassof";
  private static readonly string TYPE_PREDICATE = "http://www.w3.org/1999/02/22-rdf-syntax-ns#type";
  private readonly float offsetSize = 0.3f;
  private bool running = false;

  private void Update()
  {
    if (running)
    {
      foreach (Node node in graph.nodeList)
      {
        if (!node.LockPosition)
        {
          node.transform.localPosition = Vector3.Lerp(node.transform.localPosition, node.hierarchicalSettings.targetLocation, Time.deltaTime * 2);
        }
      }
    }
  }

  public override void CalculateLayout()
  {
    ResetNodes();
    graph.SortNodes();
    CalculateHierarchicalLevels();
    SortNodeList();
    CalculatePositions();
    running = true;
  }

  private void ResetNodes()
  {
    graph.nodeList.ForEach((Node node) => node.hierarchicalSettings.Reset());
  }

  private void CalculateHierarchicalLevels()
  {
    List<Edge> subClassOfEdgeList = graph.edgeList.FindAll(edge => edge.uri.ToLower() == SUBCLASS_OF_PREDICATE);
    Node initialNode = null;
    if (subClassOfEdgeList.Count == 0)
    {
      Debug.Log("Not a class hiearchy");
      return;
    }
    else
    {
      initialNode = subClassOfEdgeList[0].displaySubject;
    }

    initialNode.hierarchicalSettings.SetLevel(0);
    SetHierarchicalLevels(initialNode);
    CalculateHierarchicalLevelsForMultipleRootNodes();
  }

  private void CalculateHierarchicalLevelsForMultipleRootNodes()
  {
    foreach (Node node in graph.nodeList)
    {
      if (!node.hierarchicalSettings.levelFound)
      {
        node.hierarchicalSettings.SetLevel(0);
        SetHierarchicalLevels(node);
      }
    }
  }

  private void CalculatePositions()
  {
    float offset = 0;
    foreach (Node node in graph.nodeList)
    {
      if (node.hierarchicalSettings.level == 0)
      {
        offset = PositionNodeLevels(node, 0, offset);
      }
    }
  }

  private void SortNodeList()
  {
    foreach (Node node in graph.nodeList)
    {
      node.connections.Sort((Edge a, Edge b) => string.Compare(a.displaySubject.textMesh.text, b.displaySubject.textMesh.text));
    }
  }

  private void SetHierarchicalLevels(Node node)
  {
    List<Node> nodesToCall = new();
    foreach (Edge edge in node.connections)
    {
      Node partnerNode = SetEdgeHierachicalLevels(node, edge);
      if (partnerNode != null)
      {
        nodesToCall.Add(partnerNode);
      }
    }

    foreach (Node n in nodesToCall)
    {
      SetHierarchicalLevels(n);
    }

    ShiftHierarchyLevels(GetLowestLevel());
  }

  private Node SetEdgeHierachicalLevels(Node node, Edge edge)
  {
    Node partnerNode = Utils.GetPartnerNode(node, edge);
    if (partnerNode.hierarchicalSettings.levelFound) return null;
    int edgeDirection = FindEdgeDirection(node, edge);

    if (edge.uri.ToLower() == SUBCLASS_OF_PREDICATE)
    {
      SetSubClassOfSettings(node, edgeDirection, partnerNode);
    }
    else if (edge.uri.ToLower() == TYPE_PREDICATE)
    {
      SetTypeSettings(node, edgeDirection, partnerNode);
    }
    else
    {
      SetOtherSettings(node, partnerNode);
    }

    partnerNode.hierarchicalSettings.SetLevel(node.hierarchicalSettings.level + edgeDirection);
    return partnerNode;
  }

  private void ShiftHierarchyLevels(int lowestLevel)
  {
    foreach (Node currentNode in graph.nodeList)
    {
      currentNode.hierarchicalSettings.level -= lowestLevel;
    }
  }

  private int GetLowestLevel()
  {
    int lowestLevel = int.MaxValue;
    foreach (Node currentNode in graph.nodeList)
    {
      if (currentNode.hierarchicalSettings.level < lowestLevel) lowestLevel = currentNode.hierarchicalSettings.level;
    }
    return lowestLevel;
  }

  private static void SetOtherSettings(Node node, Node partnerNode)
  {
    partnerNode.hierarchicalSettings.hierarchicalType = Hierarchical.HierarchicalType.Other;
    partnerNode.hierarchicalSettings.parent = node;
    node.hierarchicalSettings.typeWithChildNodes = true;
    partnerNode.hierarchicalSettings.otherCount = 0;
  }

  private static void SetTypeSettings(Node node, int edgeDirection, Node partnerNode)
  {
    if (edgeDirection == 1)
    {
      partnerNode.hierarchicalSettings.hierarchicalType = Hierarchical.HierarchicalType.Type;
      partnerNode.hierarchicalSettings.parent = node;
    }
    else
    {
      partnerNode.hierarchicalSettings.hierarchicalType = Hierarchical.HierarchicalType.Other;
    }
    partnerNode.hierarchicalSettings.typeCount = 0;
    partnerNode.hierarchicalSettings.otherCount = 0;
  }

  private static void SetSubClassOfSettings(Node node, int edgeDirection, Node partnerNode)
  {
    partnerNode.hierarchicalSettings.hierarchicalType = Hierarchical.HierarchicalType.SubClassOf;
    if (edgeDirection == 1)
    {
      partnerNode.hierarchicalSettings.parent = node;
    }
    partnerNode.hierarchicalSettings.typeCount = 0;
    partnerNode.hierarchicalSettings.otherCount = 0;
  }

  private int FindEdgeDirection(Node node, Edge edge)
  {
    return (Utils.IsSubjectNode(node, edge) && edge.uri.ToLower() == SUBCLASS_OF_PREDICATE) ? -1 : 1;
  }

  public float PositionNodeLevels(Node node, int level, float subClassOfOffset)
  {
    float newSubClassOfOffset = subClassOfOffset;
    if (!node.LockPosition)
    {
      Node parentNode = node.hierarchicalSettings.parent;
      if (node.hierarchicalSettings.hierarchicalType == Hierarchical.HierarchicalType.Type && parentNode != null)
      {
        SetTypePositionSettings(node, level, parentNode);

      }
      else if (node.hierarchicalSettings.hierarchicalType == Hierarchical.HierarchicalType.Other && parentNode != null)
      {
        SetOtherPositionSettings(node, level, parentNode);
      }
      else
      {
        newSubClassOfOffset = SetSubClassOffPositionSettings(node, level, subClassOfOffset, newSubClassOfOffset);
      }
      node.hierarchicalSettings.subClassOfOffset = subClassOfOffset;
      node.hierarchicalSettings.positionSet = true;
    }

    int nextLevel = level + 1;
    foreach (Edge edge in node.connections)
    {
      Node childNode = Utils.GetPartnerNode(node, edge);
      if (NodeOfLevelNeedsUpdate(childNode, nextLevel))
      {
        newSubClassOfOffset = PositionNodeLevels(childNode, nextLevel, newSubClassOfOffset);
      }
    }
    return newSubClassOfOffset;
  }

  private float SetSubClassOffPositionSettings(Node node, int level, float subClassOfOffset, float newSubClassOfOffset)
  {
    SetNodePosition(node, level, subClassOfOffset, 0, 0);
    if (node.hierarchicalSettings.hierarchicalType == Hierarchical.HierarchicalType.SubClassOf)
    {
      newSubClassOfOffset += offsetSize;
    }
    return newSubClassOfOffset;
  }

  private void SetOtherPositionSettings(Node node, int level, Node parentNode)
  {
    int typeDepth;
    Node typeParentNode = FindFirstNonOtherParent(node.hierarchicalSettings.parent);
    if (typeParentNode.hierarchicalSettings.parent != null)
    {
      typeDepth = typeParentNode.hierarchicalSettings.parent.hierarchicalSettings.typeCount;
    }
    else
    {
      typeDepth = typeParentNode.hierarchicalSettings.typeCount;
    }
    node.hierarchicalSettings.Test = typeParentNode;
    int otherDepth = ++typeParentNode.hierarchicalSettings.otherCount;
    float subClassOfOffset = typeParentNode.hierarchicalSettings.subClassOfOffset - offsetSize;
    SetNodePosition(node, typeParentNode.hierarchicalSettings.level + 1, subClassOfOffset, typeDepth, otherDepth);
  }

  private static Node FindFirstNonOtherParent(Node node)
  {
    Node previousNode = node;
    for (int i = 0; i < 10; i++)
    {
      if (previousNode.hierarchicalSettings.hierarchicalType != Hierarchical.HierarchicalType.Other || previousNode == previousNode.hierarchicalSettings.parent)
      {
        break;
      }
      previousNode = previousNode.hierarchicalSettings.parent;
    }
    return previousNode;
  }

  private void SetTypePositionSettings(Node node, int level, Node parentNode)
  {
    int typeDepth = ++parentNode.hierarchicalSettings.typeCount;
    float subClassOfOffset = parentNode.hierarchicalSettings.subClassOfOffset;

    // Reserve space for child nodes
    if (node.hierarchicalSettings.typeWithChildNodes)
    {
      parentNode.hierarchicalSettings.typeCount++;
    }
    SetNodePosition(node, level, subClassOfOffset, typeDepth, 0);
  }

  private void SetNodePosition(Node node, int level, float subClassOfOffset, int typeDepth, int otherDepth)
  {
    node.hierarchicalSettings.targetLocation =
      new Vector3(0, typeDepth * offsetSize, subClassOfOffset) + new Vector3((level * (offsetSize)) + (otherDepth * offsetSize), 0, 0);
  }

  private static bool NodeOfLevelNeedsUpdate(Node node, int level)
  {
    return node.hierarchicalSettings.level == level && !node.hierarchicalSettings.positionSet;
  }

  public override void Stop()
  {
    running = false;
  }
}
