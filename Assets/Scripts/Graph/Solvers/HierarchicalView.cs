using System.Collections.Generic;
using UnityEngine;

public class HierarchicalView : BaseLayoutAlgorithm
{
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
    CalculateHierarchicalLevels();
    CalculatePositions();
    running = true;
  }

  private void CalculateHierarchicalLevels()
  {
    Node initialNode = graph.nodeList[0];
    initialNode.SetHierarchicalLevel(0);
    SetHierarchicaLevels(initialNode);
    foreach (Node node in graph.nodeList)
    {
      if (!node.hierarchicalSettings.levelFound)
      {
        node.SetHierarchicalLevel(0);
        SetHierarchicaLevels(node);
      }
    }
  }

  private void CalculatePositions()
  {
    float counter = 0;
    foreach (Node node in graph.nodeList)
    {
      if (node.hierarchicalSettings.level == 0)
      {
        PositionNodeLayer(node, 0, new Vector3(0, 0, counter));
        counter++;
      }
    }
  }

  private void ResetNodes()
  {
    foreach (Node node in graph.nodeList)
    {
      node.hierarchicalSettings.Reset();
    }
  }

  private void SetHierarchicaLevels(Node node)
  {
    List<Node> nodesToCall = new();
    foreach (Edge edge in node.connections)
    {
      Node other = Utils.GetPartnerNode(node, edge);
      int edgeDirection = FindEdgeDirection(node, edge);

      if (!other.hierarchicalSettings.levelFound)
      {
        other.SetHierarchicalLevel(node.hierarchicalSettings.level + edgeDirection);
        nodesToCall.Add(other);
      }
    }
    foreach (Node n in nodesToCall)
    {
      SetHierarchicaLevels(n);
    }
    ShiftHierarchyLevels(GetLowestLevel());
  }

  private static int FindEdgeDirection(Node node, Edge edge)
  {
    return Utils.IsSubjectNode(node, edge) ? -1 : 1;
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

  public void PositionNodeLayer(Node node, int level, Vector3 offset)
  {
    int nextLevel = level + 1;
    int previousLevel = level - 1;
    int amountOfChildNodesNextLevel = CountOfChildsForLevel(node, nextLevel);
    int amountOfChildNodesPreviousLevel = CountOfChildsForLevel(node, previousLevel);

    Vector3 newOffset = SetNodePosition(node, level, offset);

    int indexNext = 0;
    int indexPrevious = 0;
    foreach (Edge edge in node.connections)
    {
      Node parnerNode = Utils.GetPartnerNode(node, edge);

      if (NodeOfLevelNeedsUpdate(parnerNode, nextLevel))
      {
        PositionNodeLayer(parnerNode, nextLevel,
           AddToOffset(nextLevel, newOffset, amountOfChildNodesNextLevel, indexNext)
        );
        indexNext++;
      }

      if (NodeOfLevelNeedsUpdate(parnerNode, previousLevel))
      {
        PositionNodeLayer(parnerNode, previousLevel,
           AddToOffset(previousLevel, newOffset, amountOfChildNodesPreviousLevel, indexPrevious)
        );
        indexPrevious++;
      }
    }
  }

  private Vector3 SetNodePosition(Node node, int level, Vector3 offset)
  {
    node.hierarchicalSettings.positionSet = true;
    if (!node.LockPosition)
    {
      node.hierarchicalSettings.targetLocation = offset + new Vector3(level * (offsetSize * 2), 0, 0);
      return offset;
    }
    else
    {
      return node.hierarchicalSettings.targetLocation - new Vector3(level * (offsetSize * 2), 0, 0);
    }
  }

  private static int CountOfChildsForLevel(Node node, int level)
  {
    int childsInLevel = 0;
    foreach (Edge edge in node.connections)
    {
      if (NodeOfLevelNeedsUpdate(edge.displayObject, level) || NodeOfLevelNeedsUpdate(edge.displaySubject, level))
      {
        childsInLevel++;
      }
    }
    return childsInLevel;
  }

  private static bool NodeOfLevelNeedsUpdate(Node node, int level)
  {
    return node.hierarchicalSettings.level == level && !node.hierarchicalSettings.positionSet;
  }

  public Vector3 AddToOffset(int level, Vector3 offset, int totalChildren, int childIndex)
  {
    if (totalChildren % 2 == 0) totalChildren--;
    Vector3 direction = new(0, 1, 0);
    if (level % 2 == 1) direction = new Vector3(0, 0, 1);

    Vector3 step = direction * offsetSize * childIndex;
    Vector3 center = direction * offsetSize * (totalChildren - 1) * 0.5f;
    return offset + (step - center);
  }

  public override void Stop()
  {
    running = false;
  }
}
