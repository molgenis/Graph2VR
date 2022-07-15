using System.Collections.Generic;
using UnityEngine;

public class HierarchicalView : BaseLayoutAlgorithm
{
   private float offsetSize = 0.3f;
   private bool running = false;
   private void Update()
   {
      if (running)
      {
         foreach (Node node in graph.nodeList)
         {
            node.transform.localPosition = Vector3.Lerp(node.transform.localPosition, node.hierarchical.targetLocation, Time.deltaTime * 2);
         }
      }
   }

   public override void CalculateLayout()
   {
      ResetNodes();

      Node initialNode = graph.nodeList[0];
      initialNode.SetHierarchicalLevel(0);
      SetHierarchicalLayers(initialNode);


      foreach (Node node in graph.nodeList)
      {
         if (!node.hierarchical.levelFound)
         {
            node.SetHierarchicalLevel(0);
            SetHierarchicalLayers(node);
         }
      }

      float offset = 0;
      foreach (Node node in graph.nodeList)
      {
         if (node.hierarchical.level == 0)
         {
            PositionNodeLayer(node, 0, new Vector3(0, 0, offset));
            offset++;
         }
      }
      running = true;
   }

   private void ResetNodes()
   {
      foreach (Node node in graph.nodeList)
      {
         node.hierarchical.levelFound = false;
         node.hierarchical.positionSet = false;
      }
   }

   public void PositionNodeLayer(Node node, int layer, Vector3 offset)
   {
      if (!node.LockPosition)
      {
         node.hierarchical.targetLocation = offset + new Vector3(layer * (offsetSize * 2), 0, 0);
      }
      else
      {
         offset = node.hierarchical.targetLocation - new Vector3(layer * (offsetSize * 2), 0, 0);
      }

      node.hierarchical.positionSet = true;
      int nextLayer = layer + 1;
      int previousLayer = layer - 1;

      int amountOfChildNodesNextLayer = 0;
      foreach (Edge edge in node.connections)
      {
         if ((edge.displayObject.hierarchical.level == nextLayer && !edge.displayObject.hierarchical.positionSet) || (edge.displaySubject.hierarchical.level == nextLayer && !edge.displaySubject.hierarchical.positionSet))
         {
            amountOfChildNodesNextLayer++;
         }
      }

      int amountOfChildNodesPreviousLayer = 0;
      foreach (Edge edge in node.connections)
      {
         if ((edge.displayObject.hierarchical.level == previousLayer && !edge.displayObject.hierarchical.positionSet) || (edge.displaySubject.hierarchical.level == previousLayer && !edge.displaySubject.hierarchical.positionSet))
         {
            amountOfChildNodesPreviousLayer++;
         }
      }

      int indexNext = 0;
      int indexPrevious = 0;
      foreach (Edge edge in node.connections)
      {
         Node childNode;
         if (node.graph.RealNodeValue(node.graphNode) == node.graph.RealNodeValue(edge.graphSubject))
         {
            childNode = edge.displayObject;
         }
         else
         {
            childNode = edge.displaySubject;
         }

         if (childNode.hierarchical.level == nextLayer && !childNode.hierarchical.positionSet)
         {
            PositionNodeLayer(childNode, nextLayer,
               AddToOffset(nextLayer, offset, amountOfChildNodesNextLayer, indexNext)
            );
            indexNext++;
         }

         if (childNode.hierarchical.level == previousLayer && !childNode.hierarchical.positionSet)
         {
            PositionNodeLayer(childNode, previousLayer,
               AddToOffset(previousLayer, offset, amountOfChildNodesPreviousLayer, indexPrevious)
            );
            indexPrevious++;
         }
      }
   }

   public Vector3 AddToOffset(int layer, Vector3 offset, int amount, int index)
   {
      if (amount % 2 == 0) amount--;

      Vector3 direction = new Vector3(0, 1, 0);
      if (layer % 2 == 1) direction = new Vector3(0, 0, 1);

      Vector3 step = direction * offsetSize * index;
      Vector3 center = direction * offsetSize * (amount - 1) * 0.5f;
      return offset + (step - center);
   }

   List<string> structurePredicats = new List<string>
   {
      "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://www.w3.org/2000/01/rdf-schema#subclassof"
   };

   private void SetHierarchicalLayers(Node node)
   {
      List<Node> nodesToCall = new List<Node>();
      foreach (Edge edge in node.connections)
      {
         int predicateDirection = structurePredicats.Contains(edge.uri.ToLower()) ? 1 : -1;
         int objectSubjectOrderDirection;

         Node other;
         if (node.graph.RealNodeValue(node.graphNode) == node.graph.RealNodeValue(edge.graphSubject))
         {
            other = edge.displayObject; // We are a subject
            objectSubjectOrderDirection = -1;
         }
         else
         {
            other = edge.displaySubject; // we are a object
            objectSubjectOrderDirection = 1;
         }

         if (!other.hierarchical.levelFound)
         {
            other.SetHierarchicalLevel(node.hierarchical.level + (predicateDirection * objectSubjectOrderDirection));
            nodesToCall.Add(other);
         }
      }
      foreach (Node n in nodesToCall)
      {
         SetHierarchicalLayers(n);
      }

      // Get lowest level
      int lowestLevel = int.MaxValue;
      foreach (Node currentNode in graph.nodeList)
      {
         if (currentNode.hierarchical.level < lowestLevel) lowestLevel = currentNode.hierarchical.level;
      }

      // Correct lowels level to 0
      foreach (Node currentNode in graph.nodeList)
      {
         currentNode.hierarchical.level = (currentNode.hierarchical.level - lowestLevel);
      }
   }

   public override void Stop()
   {
      running = false;
   }
}
