using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using VDS.RDF;
using VDS.RDF.Query;

public class Graph : MonoBehaviour
{
   public BaseLayoutAlgorithm layout = null;
   public BoundingSphere boundingSphere;
   public GameObject edgePrefab;
   public GameObject nodePrefab;
   public Canvas menu;

   public List<Edge> edgeList = new List<Edge>();
   public List<Node> nodeList = new List<Node>();
   public List<string> translatablePredicates = new List<string>();
   public OrderedDictionary orderBy = new OrderedDictionary();
   private SparqlResultSet lastResults = null;
   public VariableNameManager variableNameManager;
   public List<Edge> selection = new List<Edge>();

   public List<Graph> subGraphs = new List<Graph>();
   public Graph parentGraph = null;
   public string creationQuery = "";

   public void SetLastResults(SparqlResultSet lastResults)
   {
      this.lastResults = lastResults;
   }

   public Graph QuerySimilarWithTriples(string triples, Vector3 position, Quaternion rotation)
   {
      Graph newGraph = Main.instance.CreateGraph();
      newGraph.parentGraph = this;
      subGraphs.Add(newGraph);
      newGraph.transform.position = position;
      newGraph.transform.rotation = rotation;
      newGraph.CreateGraphByTriples(triples);
      return newGraph;
   }

   public void QuerySimilarPatternsSingleLayer()
   {
      string triples = GetTriplesString();
      QuerySimilarWithTriples(triples, new Vector3(0, 0, 2), Quaternion.identity);
   }

   public string GetTriplesString()
   {
      Debug.Log("Get Selection: ");
      Debug.Log(selection.Aggregate(string.Empty, (accum, edge) => accum += edge.GetQueryString()));
      return selection.Aggregate(string.Empty, (accum, edge) => accum += edge.GetQueryString());
   }

   public string RealNodeValue(INode node)
   {
      switch (node.NodeType)
      {
         case NodeType.GraphLiteral:
         case NodeType.Literal:
            return GetLiteralValue(node);
         case NodeType.Uri:
            return $"<{(node as IUriNode).Uri.ToString()}>";
         case NodeType.Blank:
            return "_:blankNode";
         case NodeType.Variable:
            return (node as IVariableNode).VariableName; // TODO: do we need a '?' here?
         default:
            return "";
      }
   }

   private string GetLiteralValue(INode node)
   {
      ILiteralNode literal = (node as ILiteralNode);
      string dataType = literal.DataType?.ToString();
      string language = literal.Language?.ToString();
      string value = literal.Value?.ToString();

      string result = $"'{value}'";
      if (language != "" && language != null)
      {
         result += $"@{language}";
      }
      if (dataType != "" && dataType != null)
      {
         result += $"^^<{dataType}>";
      }
      return result;
   }

   public void QuerySimilarPatternsMultipleLayers()
   {
      string triples = GetTriplesString();
      Debug.Log("QuerySimilarPatternsMultipleLayers");
      void QuerySimilarPatternsMultipleLayersCallback(SparqlResultSet results, string query)
      {
         Debug.Log("QuerySimilarPatternsMultipleLayersCallback");
         UnityMainThreadDispatcher.Instance().Enqueue(() =>
         {
            lastResults = results;
            Quaternion rotation = Camera.main.transform.rotation;
            Vector3 offset = transform.position + (rotation * new Vector3(0, 0, 1 + boundingSphere.size));
            Debug.Log("QuerySimilarPatternsMultipleLayersCallback results: " + results.Count);
            foreach (SparqlResult result in results)
            {
               string constructQuery = triples;
               foreach (var node in result)
               {
                  constructQuery = constructQuery.Replace("?" + node.Key, RealNodeValue(node.Value));
               }

               Graph newGraph = QuerySimilarWithTriples(constructQuery, offset, Quaternion.identity);
               offset += rotation * new Vector3(0, 0, 0.5f);
               SetupNewGraph(newGraph, query, rotation, results);
            }
         });
      }

      QueryService.Instance.QuerySimilarPatternsMultipleLayers(triples, orderBy, QuerySimilarPatternsMultipleLayersCallback);
   }

   private void SetupNewGraph(Graph newGraph, string query, Quaternion rotation, SparqlResultSet results)
   {
      newGraph.creationQuery = query;
      newGraph.gameObject.GetComponent<FruchtermanReingold>().enabled = false;
      SemanticPlanes planes = newGraph.gameObject.GetComponent<SemanticPlanes>();
      planes.lookDirection = rotation;
      planes.parentGraph = this;
      planes.variableNameLookup = results;
      planes.enabled = true;
      newGraph.layout = planes;
      newGraph.boundingSphere.isFlat = true;
      newGraph.boundingSphere.GetComponent<Renderer>().forceRenderingOff = true;
   }

   public void AddToSelection(Edge toAdd)
   {
      Debug.Log("Add to Selection: " + toAdd.uri);
      selection.Add(toAdd);
   }

   public void RemoveFromSelection(Edge toRemove)
   {
      Debug.Log("Remove to Selection: " + toRemove.uri);
      // Todo: try catch?
      selection.Remove(toRemove);
   }

   public void GetOutgoingPredicats(string URI, SparqlResultsCallback sparqlResultsCallback)
   {
      if (URI == "") return;
      QueryService.Instance.GetOutgoingPredicats(URI, sparqlResultsCallback);
   }

   public void GetIncomingPredicats(string objectValue, SparqlResultsCallback sparqlResultsCallback)
   {
      if (objectValue == "") return;
      QueryService.Instance.GetIncomingPredicats(objectValue, sparqlResultsCallback);
   }

   public void CollapseGraph(Node node)
   {
      CollapseIncomingGraph(node);
      CollapseOutgoingGraph(node);
   }

   public void CollapseIncomingGraph(Node node)
   {
      CollapseGraph(node, RemoveIncoming);
   }

   private void RemoveIncoming(Node node, Edge edge)
   {
      if (edge.displayObject == node && edge.displaySubject.connections.Count == 1)
      {
         RemoveNode(edge.displaySubject);
         node.connections.Remove(edge);
      }
   }

   public void CollapseOutgoingGraph(Node node)
   {
      CollapseGraph(node, RemoveOutgoing);
   }

   private void RemoveOutgoing(Node node, Edge edge)
   {
      if (edge.displaySubject == node && edge.displayObject.connections.Count == 1)
      {
         RemoveNode(edge.displayObject);
         node.connections.Remove(edge);
      }
   }

   private void CollapseGraph(Node node, Action<Node, Edge> removeFunction)
   {
      // Reverse iterate so we can savely remove items from the list while doing the iteration
      for (int i = node.connections.Count - 1; i >= 0; i--)
      {
         Edge edge = node.connections[i];
         removeFunction(node, edge);
      }
   }

   public void ExpandGraph(Node node, string uri, bool isOutgoingLink)
   {
      QueryService.Instance.ExpandGraph(node, uri, isOutgoingLink, ((graph, refinmentGraph, state) =>
      {
         if (state != null)
         {
            //Todo: Fix this error - it still occurs sometimes (graph = null)
            Debug.Log(graph);
            Debug.Log(state);
            Debug.Log(((AsyncError)state).Error);
         }

         // To draw new elements to unity we need to be on the main Thread
         UnityMainThreadDispatcher.Instance().Enqueue(() =>
     {
        foreach (Triple triple in graph.Triples)
        {
           AddTriple(triple);
        }

        // Add refinement
        // Add label
        //List<INode> refinmentNodes = refinmentGraph.Nodes.ToList<INode>();
        foreach (Node nodeToRefine in nodeList)
        {
           if (nodeToRefine.graphNode.NodeType != NodeType.Uri) continue;

           Dictionary<string, List<string>> results = QueryService.Instance.RefineNode(refinmentGraph, nodeToRefine.uri);
           List<string> images = results["images"];
           if (results["labels"].Count > 0)
           {
              nodeToRefine.SetLabel(results["labels"].First());
           }
           nodeToRefine.SetImageFromList(images);
        }
     });
      }));
   }

   private void AddTriple(Triple triple)
   {
      AddObjects(triple);
      AddSubjects(triple);
      AddEdges(triple);
      layout.CalculateLayout();
   }

   private void AddEdges(Triple triple)
   {
      if (!edgeList.Find(edge => edge.Equals(triple.Subject, triple.Predicate, triple.Object)))
      {
         CreateEdge(triple.Subject, triple.Predicate, triple.Object);
      }
   }

   private void AddSubjects(Triple triple)
   {
      if (IsNonExistantNode(triple.Subject))
      {
         CreateNode(triple.Subject.ToString(), triple.Subject);
      }
   }

   private void AddObjects(Triple triple)
   {
      if (IsNonExistantNode(triple.Object))
      {
         CreateNode(triple.Object.ToString(), triple.Object);
      }
   }

   private bool IsNonExistantNode(INode node)
   {
      return !nodeList.Find(graphicalNode => graphicalNode.graphNode.Equals(node));
   }

   public string GetShortName(string uri)
   {
      if (QueryService.Instance.defaultNamespace.ReduceToQName(uri, out string qName))
      {
         return qName;
      }

      string[] splittedHashUri = uri.Split('#');
      if (splittedHashUri.Length != 1)
      {
         return splittedHashUri[splittedHashUri.Length - 1];
      }

      string[] splittedBackslashUri = uri.Split('/');
      if (splittedBackslashUri.Length != 1)
      {
         return splittedBackslashUri[splittedBackslashUri.Length - 1];
      }
      else
      {
         return uri;
      }
   }

   public string CleanInfo(string str)
   {
      return str.TrimStart('<', '\'').TrimEnd('>', '\'');
   }

   public void CreateGraphByTriples(string triples)
   {
      QueryService.Instance.QueryByTriples(triples, RebuildGraphCallback);
   }

   public void CreateGraphBySparqlQuery(string query)
   {
      QueryService.Instance.ExecuteQuery(query, RebuildGraphCallback);
   }

   private void RebuildGraphCallback(IGraph resultGraph, object state)
   {
      if (resultGraph == null || resultGraph.Triples == null || resultGraph.Triples.Count == 0)
      {
         Destroy(gameObject);
      }
      else
      {
         resultGraph.NamespaceMap.Import(QueryService.Instance.defaultNamespace);
         UnityMainThreadDispatcher.Instance().Enqueue(() =>
         {
            BuildByIGraph(resultGraph);

            SemanticPlanes plane = gameObject.GetComponent<SemanticPlanes>();
            if (plane.enabled && this.layout == plane)
            {
               plane.CalculateLayout();
            }
         });
      }
   }

   // Builds a new graph out of an IGraph, deletes the old one
   private void BuildByIGraph(IGraph iGraph)
   {
      Clear();

      foreach (INode node in iGraph.Nodes)
      {
         CreateNode(node.ToString(), node);
      }

      foreach (Triple triple in iGraph.Triples)
      {
         CreateEdge(triple.Subject, triple.Predicate, triple.Object);
      }

      layout.CalculateLayout();
   }

   public void Clear()
   {
      foreach (Node node in nodeList)
      {
         Destroy(node.gameObject);
      }

      foreach (Edge edge in edgeList)
      {
         Destroy(edge.gameObject);
      }

      nodeList.Clear();
      edgeList.Clear();
   }

   private void Awake()
   {
      variableNameManager = new VariableNameManager();
   }

   public Edge CreateEdge(Node from, string uri, Node to)
   {
      Edge edge = InitializeEdge(uri, from, to);
      edgeList.Add(edge);
      to.AddConnection(edge);
      from.AddConnection(edge);
      return edge;
   }

   public void CreateEdge(INode from, INode uri, INode to)
   {
      Node fromNode = GetByINode(from);
      Node toNode = GetByINode(to);

      if (fromNode == null || toNode == null)
      {
         Debug.Log("The Subject and Object needs to be defined to create a edge");
         return;
      }

      Edge edge = InitializeEdge(uri.ToString(), fromNode, toNode);
      edge.graphPredicate = uri;
      edge.graphSubject = from;
      edge.graphObject = to;

      edgeList.Add(edge);
      toNode.AddConnection(edge);
      fromNode.AddConnection(edge);
   }

   private Edge InitializeEdge(string uri, Node from, Node to)
   {
      GameObject clone = GetEdgeClone();
      clone.name = "Edge: " + uri;
      Edge edge = clone.GetComponent<Edge>();
      edge.graph = this;
      edge.uri = uri;
      edge.displaySubject = from;
      edge.displayObject = to;

      // Check if its self referencing
      if (from.uri != null && from.uri != "" && from.uri == to.uri)
      {
         edge.lineType = Edge.LineType.Circle;
      }
      else
      {
         edge.UpdateEdgeLines();
      }

      return edge;
   }

   private GameObject GetEdgeClone()
   {
      GameObject clone = Instantiate<GameObject>(edgePrefab);
      clone.transform.SetParent(transform);
      clone.transform.localPosition = Vector3.zero;
      clone.transform.localRotation = Quaternion.identity;
      clone.transform.localScale = Vector3.one;
      return clone;
   }

   public void CreateNode(string value, INode iNode)
   {
      GameObject clone = Instantiate<GameObject>(nodePrefab);
      clone.name = "Node " + value;
      clone.transform.SetParent(transform);
      clone.transform.localPosition = UnityEngine.Random.insideUnitSphere * 3f;
      clone.transform.localRotation = Quaternion.identity;
      clone.transform.localScale = Vector3.one * 0.05f;
      Node node = CreateNodeFromClone(value, clone);
      node.graphNode = iNode;
   }

   public Node CreateNode(string value, Vector3 position)
   {
      GameObject clone = Instantiate<GameObject>(nodePrefab);
      clone.name = "Node " + value;
      clone.transform.SetParent(transform);
      clone.transform.position = position;
      clone.transform.localRotation = Quaternion.identity;
      clone.transform.localScale = Vector3.one * 0.05f;
      Node node = CreateNodeFromClone(value, clone);
      NodeFactory nodeFactory = new NodeFactory();
      node.graphNode = nodeFactory.CreateUriNode(new Uri(value));
      return node;
   }

   private Node CreateNodeFromClone(string value, GameObject clone)
   {
      Node node = clone.AddComponent<Node>();

      node.graph = this;
      node.SetURI(value);
      node.SetLabel(value);
      nodeList.Add(node);
      return node;
   }

   public void AddNodeFromDatabase(Node variableNode = null)
   {
      AutocompleteHandeler.Instance.SearchForNode((string label, string uri) =>
      {
         Vector3 nodeSpawnPosition = GameObject.FindGameObjectWithTag("LeftController").transform.position;
         Node preExistingNode = null;
         foreach (Node node in nodeList)
         {
            if (node.uri != "" && uri == node.uri)
            {
               preExistingNode = node;
            }
            else if (node.uri == "" && label == node.label)
            {
               preExistingNode = node;
            }
         }
         if (preExistingNode != null)
         {
            preExistingNode.gameObject.transform.position = nodeSpawnPosition;
         }
         else
         {
            Node newNode = CreateNode(uri, nodeSpawnPosition);
            newNode.SetLabel(label);
         }
      }, variableNode);
   }

   public Node GetByINode(INode iNode)
   {
      return nodeList.Find((Node node) => node.graphNode.Equals(iNode));
   }

   // Removes a node from the graph. This will also remove all the edges leading to this node.
   // Settings to removeEmptyLeaves to true will remove any connected node that only has this node as a connection.
   public void RemoveNode(Node node, bool removeEmptyLeaves = false)
   {
      if (node != null)
      {
         // Reverse iterate so we can savely remove items from the list while doing the iteration
         for (int i = node.connections.Count - 1; i >= 0; i--)
         {
            Edge edge = node.connections[i];
            edge.displayObject.connections.Remove(edge);
            edge.displaySubject.connections.Remove(edge);
            edgeList.Remove(edge);
            Destroy(edge.gameObject);
            Node otherNode = edge.displayObject == node ? edge.displaySubject : edge.displayObject;
            if (removeEmptyLeaves && otherNode.connections.Count == 0)
            {
               RemoveNode(otherNode);
            }
         }

         nodeList.Remove(node);
         Destroy(node.gameObject);
      }
   }

   public void RemoveEdge(Edge edge)
   {
      edgeList.Remove(edge);
      edge.Remove();

      Destroy(edge.gameObject);
   }

   public void Remove()
   {
      if (boundingSphere != null)
      {
         Destroy(boundingSphere.gameObject);
      }
      if (gameObject != null)
      {
         Destroy(gameObject);
      }
   }

   public void RemoveSubGraphs()
   {
      foreach (Graph graph in subGraphs)
      {
         if (graph != null) graph.Remove();
      }
      subGraphs.Clear();
   }

   public void RemoveGraphsOfSameQuery()
   {
      if (parentGraph != null)
      {
         List<Graph> subGraphsToRemove = new List<Graph>(parentGraph.subGraphs);
         foreach (Graph graph in subGraphsToRemove)
         {
            if (graph != this && graph.creationQuery != null && graph.creationQuery == creationQuery)
            {
               graph.Remove();
               parentGraph.subGraphs.Remove(graph);
            }
         }
         this.creationQuery = null;
      }
   }

   public void SwitchLayout<T>()
   {
      foreach (BaseLayoutAlgorithm baseLayout in GetComponents<BaseLayoutAlgorithm>())
      {
         baseLayout.enabled = false;
      }

      BaseLayoutAlgorithm activeLayout = GetComponent<T>() as BaseLayoutAlgorithm;
      layout = activeLayout;
      activeLayout.enabled = true;
   }

   public void pinAllNodes(bool pin)
   {
      foreach (Node nodeToPin in nodeList)
      {
         LeanTween.cancel(nodeToPin.gameObject);
         if (pin)
         {
            LeanTween.value(nodeToPin.gameObject, 0.4f, 0.2f, 0.5f).setOnUpdate(value => nodeToPin.transform.Find("Nail").GetComponent<NailRotation>().offset = value);
            {
               nodeToPin.LockPosition = pin;
            }
         }
         else
         {
            LeanTween.value(nodeToPin.gameObject, 0.2f, 0.4f, 0.3f).setOnUpdate(value => nodeToPin.transform.Find("Nail").GetComponent<NailRotation>().offset = value).setOnComplete(() =>
            {
               nodeToPin.LockPosition = pin;
            });
         }
      }
   }
}
