using System.Collections.Generic;
using UnityEngine;

public class ClassHierarchy : BaseLayoutAlgorithm
{
   private float offsetSize = 0.3f;
   private string subClassOfPredicate = "http://www.w3.org/2000/01/rdf-schema#subclassof";
   private string typePredicate = "http://www.w3.org/1999/02/22-rdf-syntax-ns#type";
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
      graph.SortNodes();

      List<Edge> subClassOfEdgeList = graph.edgeList.FindAll(edge => edge.uri.ToLower() == subClassOfPredicate);
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

      initialNode.SetHierarchicalLevel(0);
      SetHierarchicalLayers(initialNode);

      // Special case: Multiple root nodes
      foreach (Node node in graph.nodeList)
      {
         if (!node.hierarchical.levelFound)
         {
            node.SetHierarchicalLevel(0);
            SetHierarchicalLayers(node);
         }
      }

      // sort??
      foreach (Node node in graph.nodeList)
      {
         node.connections.Sort((Edge a, Edge b) => string.Compare(a.displaySubject.textMesh.text, b.displaySubject.textMesh.text));
      }

      // Set positions
      float offset = 0;
      foreach (Node node in graph.nodeList)
      {
         if (node.hierarchical.level == 0)
         {
            offset = PositionNodeLayer(node, 0, offset);
         }
      }
      running = true;
   }

   private void ResetNodes()
   {
      foreach (Node node in graph.nodeList)
      {
         node.hierarchical.Reset();
      }
   }

   public float PositionNodeLayer(Node node, int layer, float offset)
   {
      float newOffset = offset;
      if (!node.LockPosition)
      {
         int typeDepth = 0;
         int otherDepth = 0;
         if (node.hierarchical.hierarchicalType == Node.HierarchicalType.Type && node.hierarchical.parent != null)
         {
            node.hierarchical.parent.hierarchical.typeCount++;
            typeDepth = node.hierarchical.parent.hierarchical.typeCount;
            offset = node.hierarchical.parent.hierarchical.offset;
            if (node.hierarchical.typeWithChildNodes)
            {
               node.hierarchical.parent.hierarchical.typeCount++;
            }
         }
         if (node.hierarchical.hierarchicalType == Node.HierarchicalType.Other && node.hierarchical.parent != null)
         {
            node.hierarchical.parent.hierarchical.otherCount++;
            if (node.hierarchical.parent.hierarchical.parent != null)
            {
               typeDepth = node.hierarchical.parent.hierarchical.parent.hierarchical.typeCount;
            }
            otherDepth = node.hierarchical.parent.hierarchical.otherCount;
            offset = node.hierarchical.parent.hierarchical.offset;
         }
         node.hierarchical.targetLocation = new Vector3(0, typeDepth * offsetSize, offset) + new Vector3((layer * (offsetSize * 2)) + (otherDepth * offsetSize), 0, 0);
         //node.transform.localPosition = new Vector3(0, typeDepth * offsetSize, offset) + new Vector3((layer * (offsetSize * 2)) + (otherDepth * offsetSize), 0, 0);
         node.hierarchical.offset = offset;
         if (node.hierarchical.hierarchicalType == Node.HierarchicalType.SubClassOf)
         {
            newOffset += offsetSize;
         }
      }

      node.hierarchical.positionSet = true;
      int nextLayer = layer + 1;

      foreach (Edge edge in node.connections)
      {
         Node childNode;

         // Are we a object or subject?
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
            newOffset = PositionNodeLayer(childNode, nextLayer, newOffset);
         }
      }
      return newOffset;
   }

   /*
   List<string> structurePredicats = new List<string>
   {
      "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://www.w3.org/2000/01/rdf-schema#subclassof"
   };
   */

   private void SetHierarchicalLayers(Node node)
   {
      List<Node> nodesToCall = new List<Node>();
      foreach (Edge edge in node.connections)
      {
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

         if (edge.uri.ToLower() == subClassOfPredicate)
         {
            other.hierarchical.hierarchicalType = Node.HierarchicalType.SubClassOf;
            if (!other.hierarchical.levelFound)
            {
               other.hierarchical.parent = node;
               other.hierarchical.typeCount = 0;
               other.hierarchical.otherCount = 0;
               other.SetHierarchicalLevel(node.hierarchical.level + objectSubjectOrderDirection);
               nodesToCall.Add(other);
            }
         }
         else if (edge.uri.ToLower() == typePredicate)
         {
            //if (objectSubjectOrderDirection == 1)
            other.hierarchical.hierarchicalType = Node.HierarchicalType.Type;
            if (!other.hierarchical.levelFound)
            {
               other.hierarchical.parent = node;
               other.hierarchical.typeCount = 0;
               other.hierarchical.otherCount = 0;
               other.SetHierarchicalLevel(node.hierarchical.level + objectSubjectOrderDirection);
               nodesToCall.Add(other);
            }
         }
         else
         {
            if (!other.hierarchical.levelFound)
            {
               other.hierarchical.parent = node;
               other.hierarchical.parent.hierarchical.typeWithChildNodes = true;
               other.hierarchical.hierarchicalType = Node.HierarchicalType.Other;
               other.hierarchical.otherCount = 0;
               other.SetHierarchicalLevel(node.hierarchical.level + objectSubjectOrderDirection);
               nodesToCall.Add(other);
            }
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
