using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using VDS.RDF;

public class SaveLoad : MonoBehaviour
{
   private static string regexSelector = "^(<[^>]+>)\\s(<[^>]+>)\\s([<|'|\"][^'\">]+[>|'|\"])";
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
         }
         writer.Flush();
      }
   }

   private static string[] ExtractNodes(string triple)
   {
      GroupCollection groups = Regex.Match(triple, regexSelector, RegexOptions.Singleline).Groups;
      if (groups.Count != 4)
      {
         Debug.Log("The Save/Load Regex is not parsing this situation: " + triple);
         return null;
      }
      else
      {
         return new string[3] {
         groups[1].Value,
         groups[2].Value,
         groups[3].Value
      };
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
            string[] split = ExtractNodes(line);
            if (split == null) continue;
            string subject = graph.CleanInfo(split[0]);
            string predicate = graph.CleanInfo(split[1]);
            string objectValue = graph.CleanInfo(split[2]);

            IUriNode subjectNode = nodeFactory.CreateUriNode(new Uri(subject));
            IUriNode predicateNode = nodeFactory.CreateUriNode(new Uri(predicate));
            INode objectNode;

            if (split[2][0] == '<')
            {
               objectNode = nodeFactory.CreateUriNode(new Uri(objectValue));
            }
            else
            {
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




