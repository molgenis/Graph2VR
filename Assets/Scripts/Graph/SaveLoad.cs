using System;
using System.IO;
using UnityEngine;
using VDS.RDF;

public class SaveLoad : MonoBehaviour
{
   private static string FileName(string name)
   {
      return Path.Combine(Application.persistentDataPath, name + ".nt");
   }

   public static void Save(Graph graph, string name)
   {
      using (StreamWriter writer = new StreamWriter(FileName(name), false))
      {
         foreach (Edge edge in graph.edgeList)
         {
            writer.WriteLine($"{graph.RealNodeValue(edge.graphSubject)} {graph.RealNodeValue(edge.graphPredicate)} {graph.RealNodeValue(edge.graphObject)} .");
            Debug.Log($"{graph.RealNodeValue(edge.graphSubject)} {graph.RealNodeValue(edge.graphPredicate)} {graph.RealNodeValue(edge.graphObject)} .");
         }
         writer.Flush();
      }
   }

   public static void Load(Graph graph, string name)
   {
      NodeFactory nodeFactory = new NodeFactory();
      using (StreamReader reader = new StreamReader(FileName(name)))
      {
         string line;
         while ((line = reader.ReadLine()) != null)
         {
            string[] split = line.Split(' '); // Needs a smart way to split lines and there can be spaces in url's and literals
            string subject = graph.CleanInfo(split[0]);
            string predicate = graph.CleanInfo(split[1]);
            string objectValue;

            IUriNode subjectNode = nodeFactory.CreateUriNode(new Uri(subject));
            IUriNode predicateNode = nodeFactory.CreateUriNode(new Uri(predicate));
            INode objectNode;

            if (split[2][0] == '<')
            {
               objectValue = graph.CleanInfo(split[2]);
               objectNode = nodeFactory.CreateUriNode(new Uri(objectValue));
            }
            else
            {
               objectValue = split[2];
               objectNode = nodeFactory.CreateLiteralNode(objectValue);
            }

            if (graph.GetByINode(subjectNode) == null)
            {
               graph.CreateNode(subject, subjectNode);
            }
            if (graph.GetByINode(objectNode) == null)
            {
               graph.CreateNode(objectValue, objectNode);
            }
            graph.CreateEdge(
               subjectNode,
               predicateNode,
               objectNode
               );
         }
      }
   }
}




