using System.Collections.Generic;
using UnityEngine;

public class HierarchicalView : BaseLayoutAlgorithm
{
   private float offsetSize = 0.3f;

   public override void CalculateLayout()
   {
      ResetNodes();

      Node initialNode = graph.nodeList[0];
      initialNode.SetHierarchicalLevel(0);
      SetHierarchicalLayers(initialNode);


      foreach (Node node in graph.nodeList)
      {
         if (!node.hierarchicalLevelFound)
         {
            node.SetHierarchicalLevel(0);
            SetHierarchicalLayers(node);
         }
      }

      float offset = 0;
      foreach (Node node in graph.nodeList)
      {
         if (node.hierarchicalLevel == 0)
         {
            PositionNodeLayer(node, 0, new Vector3(0, 0, offset));
            offset++;
         }
      }
   }

   private void ResetNodes()
   {
      foreach (Node node in graph.nodeList)
      {
         node.hierarchicalLevelFound = false;
         node.hierarchicalPositionSet = false;
      }
   }

   public void PositionNodeLayer(Node node, int layer, Vector3 offset)
   {
      if (!node.LockPosition)
      {
         node.transform.localPosition = offset + new Vector3(layer * (offsetSize * 2), 0, 0);
      }
      else
      {
         offset = node.transform.localPosition - new Vector3(layer * (offsetSize * 2), 0, 0);
      }

      node.hierarchicalPositionSet = true;
      int nextLayer = layer + 1;
      int previousLayer = layer - 1;

      int amountOfChildNodesNextLayer = 0;
      foreach (Edge edge in node.connections)
      {
         if ((edge.displayObject.hierarchicalLevel == nextLayer && !edge.displayObject.hierarchicalPositionSet) || (edge.displaySubject.hierarchicalLevel == nextLayer && !edge.displaySubject.hierarchicalPositionSet))
         {
            amountOfChildNodesNextLayer++;
         }
      }

      int amountOfChildNodesPreviousLayer = 0;
      foreach (Edge edge in node.connections)
      {
         if ((edge.displayObject.hierarchicalLevel == previousLayer && !edge.displayObject.hierarchicalPositionSet) || (edge.displaySubject.hierarchicalLevel == previousLayer && !edge.displaySubject.hierarchicalPositionSet))
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

         if (childNode.hierarchicalLevel == nextLayer && !childNode.hierarchicalPositionSet)
         {
            PositionNodeLayer(childNode, nextLayer,
               AddToOffset(nextLayer, offset, amountOfChildNodesNextLayer, indexNext)
            );
            indexNext++;
         }

         if (childNode.hierarchicalLevel == previousLayer && !childNode.hierarchicalPositionSet)
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

         if (!other.hierarchicalLevelFound)
         {
            other.SetHierarchicalLevel(node.hierarchicalLevel + (predicateDirection * objectSubjectOrderDirection));
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
         if (currentNode.hierarchicalLevel < lowestLevel) lowestLevel = currentNode.hierarchicalLevel;
      }

      // Correct lowels level to 0
      foreach (Node currentNode in graph.nodeList)
      {
         currentNode.hierarchicalLevel = (currentNode.hierarchicalLevel - lowestLevel);
      }
   }

   public override void Stop()
   {
   }
}
