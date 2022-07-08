using System.Collections.Generic;
using UnityEngine;

public class HierarchicalView : BaseLayoutAlgorithm
{
   private float offsetSize = 0.3f;

   public override void CalculateLayout()
   {
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

      // Get lowest level
      int lowestLevel = int.MaxValue;
      foreach (Node node in graph.nodeList)
      {
         if (node.hierarchicalLevel < lowestLevel) lowestLevel = node.hierarchicalLevel;
      }

      // Correct lowels level to 0
      foreach (Node node in graph.nodeList)
      {
         node.hierarchicalLevel = (node.hierarchicalLevel - lowestLevel);
      }

      float offset = 0;
      foreach (Node node in graph.nodeList)
      {
         if (node.hierarchicalLevel == 0)
         {
            PositionNodeLayer(node, 0, new Vector3(0, 0, offset));
            offset += offsetSize;
         }
      }

   }

   public void PositionNodeLayer(Node node, int layer, Vector3 offset)
   {
      node.transform.localPosition = offset + new Vector3(layer * (offsetSize * 2), 0, 0);
      node.hierarchicalPositionSet = true;
      int nextLayer = layer + 1;

      int amountOfChildNodes = 0;
      foreach (Edge edge in node.connections)
      {
         if ((edge.displayObject.hierarchicalLevel == nextLayer && !edge.displayObject.hierarchicalPositionSet) || (edge.displaySubject.hierarchicalLevel == nextLayer && !edge.displaySubject.hierarchicalPositionSet))
         {
            amountOfChildNodes++;
         }
      }

      int index = 0;
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
               AddToOffset(nextLayer, offset, amountOfChildNodes, index)
               );
            index++;
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
   }

   public override void Stop()
   {
   }
}
