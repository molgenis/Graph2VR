using System;
using System.Collections.Generic;
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
  public List<string> orderBy = new List<string>();
  private SparqlResultSet lastResults = null;
  public VariableNameManager variableNameManager;
  public List<Edge> selection = new List<Edge>();

  public List<Graph> subGraphs = new List<Graph>();
  public Graph parentGraph = null;
  public string creationQuery = "";

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

  private string GetTriplesString()
  {
    string triples = string.Empty;
    foreach (Edge edge in selection)
    {
      triples += edge.GetQueryString();
    }
    return triples;
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
    string dataType = (node as ILiteralNode).DataType?.ToString();
    if (dataType == "" || dataType == null)
    {
      return $"\"{ (node as ILiteralNode).Value}\"";
    }
    else
    {
      return $"\"{ (node as ILiteralNode).Value}\"^^<{dataType}>";
    }
  }

  public void QuerySimilarPatternsMultipleLayers()
  {
    string triples = GetTriplesString();
    // FIXME: test code
    // triples = " ?variable1 <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> ?variable2 .";
    // triples += "?variable1 <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.semanticweb.org/alexander/ontologies/2021/6/untitled-ontology-479#Study> .";
    lastResults = QueryService.Instance.QuerySimilarPatternsMultipleLayers(triples, orderBy, out string query);

    Quaternion rotation = Camera.main.transform.rotation;
    Vector3 offset = transform.position + (rotation * new Vector3(0, 0, 1 + boundingSphere.size));
    foreach (SparqlResult result in lastResults)
    {
      string constructQuery = triples;
      foreach (var node in result)
      {
        constructQuery = constructQuery.Replace("?" + node.Key, RealNodeValue(node.Value));
      }

      Graph newGraph = QuerySimilarWithTriples(constructQuery, offset, Quaternion.identity);
      if (newGraph.nodeList.Count > 0)
      {
        offset += rotation * new Vector3(0, 0, 0.5f);
        CreateNewGraph(newGraph, query, rotation);
      }
    }
  }

  private void CreateNewGraph(Graph newGraph, string query, Quaternion rotation)
  {
    newGraph.creationQuery = query;
    newGraph.gameObject.GetComponent<FruchtermanReingold>().enabled = false;
    SemanticPlanes planes = newGraph.gameObject.GetComponent<SemanticPlanes>();
    planes.lookDirection = rotation;
    planes.parentGraph = this;
    planes.variableNameLookup = lastResults;
    planes.enabled = true;
    newGraph.layout = planes;
    newGraph.boundingSphere.isFlat = true;
    planes.CalculateLayout();
    newGraph.boundingSphere.GetComponent<Renderer>().forceRenderingOff = true;
  }

  public void AddToSelection(Edge toAdd)
  {
    selection.Add(toAdd);
  }

  public void RemoveFromSelection(Edge toRemove)
  {
    // Todo: try catch?
    selection.Remove(toRemove);
  }

  // NOTE: this is a development function
  /*
  public List<string> GetSubjects()
  {
    SparqlRemoteEndpoint endpoint = GetEndPoint();
    lastResults = endpoint.QueryWithResultSet(
        "select distinct ?s where { ?s ?p ?o } LIMIT 10"
        );

    List<string> results = new List<string>();
    // Fill triples list 
    foreach (SparqlResult result in lastResults)
    {
      result.TryGetValue("s", out INode subject);
      result.TryGetValue("p", out INode predicate);
      result.TryGetValue("o", out INode o);

      if (subject != null)
      {
        results.Add(subject.ToString());
      }
    }
    return results;
  }
  */

  public Dictionary<string, Tuple<string, int>> GetOutgoingPredicats(string URI)
  {
    if (URI == "") return null;
    try
    {
      SparqlResultSet results = QueryService.Instance.GetOutgoingPredicats(URI);
      lastResults = results; // FIXME: Is lastResults important?
      return GetPredicatsList(results);
    }
    catch (Exception e)
    {
      Debug.Log("GetOutgoingPredicats error: " + e.Message);
      Debug.Log("URI: " + URI);
      Debug.Log(lastResults);
    }
    return null;
  }

  public Dictionary<string, Tuple<string, int>> GetIncomingPredicats(string URI)
  {
    if (URI == "") return null;
    try
    {
      SparqlResultSet results = QueryService.Instance.GetIncomingPredicats(URI);
      lastResults = results;
      return GetPredicatsList(results);
    }
    catch (Exception e)
    {
      Debug.Log("GetIncomingPredicats error: " + e.Message);
      Debug.Log("URI: " + URI);
      Debug.Log(lastResults);
      return null;
    }
  }

  private Dictionary<string, Tuple<string, int>> GetPredicatsList(SparqlResultSet sparqlResults)
  {
    Dictionary<string, Tuple<string, int>> results = new Dictionary<string, Tuple<string, int>>();
    foreach (SparqlResult result in sparqlResults)
    {
      result.TryGetValue("p", out INode predicate);
      result.TryGetValue("count", out INode count);
      result.TryGetValue("label", out INode labelNode);

      string label = labelNode != null ? labelNode.ToString() : "";

      if (predicate != null)
      {
        if (!results.ContainsKey(predicate.ToString()))
        {
          results.Add(predicate.ToString(), new Tuple<string, int>(label, int.Parse(count.ToString())));
        }
        else if (!DoesFirstResultContainLanguageCode(results, predicate))
        {
          results[predicate.ToString()] = new Tuple<string, int>(label, int.Parse(count.ToString()));
        }
      }
    }
    return results;
  }

  private Boolean DoesFirstResultContainLanguageCode(Dictionary<string, Tuple<string, int>> results, INode predicate)
  {
    return results[predicate.ToString()].Item1.Contains("@" + Main.instance.languageCode);
  }

  public void CollapseGraph(Node node)
  {
    CollapseIncomingGraph(node);
    CollapseOutgoingGraph(node);
  }

  public void CollapseIncomingGraph(Node node)
  {
    // Reverse iterate so we can savely remove items from the list while doing the iteration
    for (int i = node.connections.Count - 1; i >= 0; i--)
    {
      Edge edge = node.connections[i];
      if (edge.displayObject == node && edge.displaySubject.connections.Count == 1)
      {
        Remove(edge.displaySubject);
        node.connections.Remove(edge);
      }
    }
  }

  public void CollapseOutgoingGraph(Node node)
  {
    // Reverse iterate so we can savely remove items from the list while doing the iteration
    for (int i = node.connections.Count - 1; i >= 0; i--)
    {
      Edge edge = node.connections[i];
      if (edge.displaySubject == node && edge.displayObject.connections.Count == 1)
      {
        Remove(edge.displayObject);
        node.connections.Remove(edge);
      }
    }
  }

  public void ExpandGraph(Node node, string uri, bool isOutgoingLink)
  {
    QueryService.Instance.ExpandGraph(node, uri, isOutgoingLink, ((graph, state) =>
    {
      if (state != null)
      {
        //Todo: Fix this error - it still occurs sometimes (graph = null)
        Debug.Log("There may be an error");
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
      });
    }));
  }


  // Return a short name if possible
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

  private string CleanInfo(string str)
  {
    // TODO: do we need: return str.TrimStart('<', '"').TrimEnd('>', '"');
    return str.TrimStart('<').TrimEnd('>');
  }

  public void CreateGraphByTriples(string triples)
  {
    IGraph graph = QueryService.Instance.QueryByTriples(triples);
    if (graph == null || graph.Triples == null || graph.Triples.Count == 0)
    {
      Destroy(gameObject);
    }
    else
    {
      BuildByIGraph(graph);
    }
  }
  public void CreateGraphBySparqlQuery(string query)
  {
    IGraph graph = QueryService.Instance.ExecuteQuery(query);
    if (graph == null || graph.Triples == null || graph.Triples.Count == 0)
    {
      Destroy(gameObject);
    }
    else
    {
      BuildByIGraph(graph);
    }
  }

  private void AddTriple(Triple triple)
  {
    // Add objects
    if (!nodeList.Find(graphicalNode => graphicalNode.graphNode.Equals(triple.Object)))
    {
      Node n = CreateNode(triple.Object.ToString(), triple.Object);
    }

    // Add subjects
    if (!nodeList.Find(graphicalNode => graphicalNode.graphNode.Equals(triple.Subject)))
    {
      Node n = CreateNode(triple.Subject.ToString(), triple.Subject);
    }

    // Add edges
    if (!edgeList.Find(edge => edge.Equals(triple.Subject, triple.Predicate, triple.Object)))
    {
      Edge e = CreateEdge(triple.Subject, triple.Predicate, triple.Object);
    }

    layout.CalculateLayout();
  }

  // Builds a new graph out of an IGraph, deletes the old one
  private void BuildByIGraph(IGraph iGraph)
  {
    Clear();
    // Add nodes
    foreach (INode node in iGraph.Nodes)
    {
      Node n = CreateNode(node.ToString(), node);
    }

    // Add edges
    foreach (Triple triple in iGraph.Triples)
    {
      Edge e = CreateEdge(triple.Subject, triple.Predicate, triple.Object);
    }

    layout.CalculateLayout();
  }
  public void Clear()
  {
    for (int i = 0; i < nodeList.Count; i++)
    {
      Destroy(nodeList[i].gameObject);
    }
    for (int i = 0; i < edgeList.Count; i++)
    {
      Destroy(edgeList[i].gameObject);
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
    GameObject clone = GetEdgeClone();

    Edge edge = InitializeEdge(clone, uri, from, to);
    edgeList.Add(edge);
    from.AddConnection(edge);
    to.AddConnection(edge);
    return edge;
  }

  public Edge CreateEdge(INode from, INode uri, INode to)
  {
    GameObject clone = GetEdgeClone();

    Node fromNode = GetByINode(from);
    Node toNode = GetByINode(to);

    if (fromNode == null || toNode == null)
    {
      Debug.Log("The Subject and Object needs to be defined to create a edge");
      return null;
    }

    Edge edge = InitializeEdge(clone, uri.ToString(), fromNode, toNode);
    edge.graphPredicate = uri;
    edge.graphSubject = from;
    edge.graphObject = to;

    edgeList.Add(edge);

    fromNode.AddConnection(edge);
    toNode.AddConnection(edge);

    return edge;
  }
  private Edge InitializeEdge(GameObject clone, string uri, Node from, Node to)
  {
    Edge edge = clone.AddComponent<Edge>();
    edge.graph = this;
    edge.uri = uri;
    edge.displaySubject = from;
    edge.displayObject = to;
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

  public Node CreateNode(string value, INode iNode)
  {
    GameObject clone = Instantiate<GameObject>(nodePrefab);
    clone.transform.SetParent(transform);
    clone.transform.localPosition = UnityEngine.Random.insideUnitSphere * 3f;
    clone.transform.localRotation = Quaternion.identity;
    clone.transform.localScale = Vector3.one * 0.05f;
    Node node = clone.AddComponent<Node>();
    node.graph = this;
    node.SetURI(value);
    node.SetLabel(value);
    nodeList.Add(node);
    node.graphNode = iNode;
    return node;
  }


  public Node CreateNode(string value, Vector3 position)
  {
    GameObject clone = Instantiate<GameObject>(nodePrefab);
    clone.transform.SetParent(transform);
    clone.transform.position = position;
    clone.transform.localRotation = Quaternion.identity;
    clone.transform.localScale = Vector3.one * 0.05f;
    Node node = clone.AddComponent<Node>();
    node.graph = this;
    node.SetURI(value);
    node.SetLabel(value);
    nodeList.Add(node);
    return node;
  }

  public Node GetByINode(INode iNode)
  {
    return nodeList.Find((Node node) => node.graphNode.Equals(iNode));
  }


  public void Hide(Node node)
  {
    if (node != null)
    {
      node.gameObject.SetActive(false);
    }
    // NOTE: is it correct only to hide edges pointing to this node? how about edges pointing away from this node?
    Edge e = edgeList.Find((Edge edge) => edge.graphObject.Equals(node.graphNode));
    if (e != null)
    {
      e.gameObject.SetActive(false);
    }
  }

  public void Hide(INode node)
  {
    Hide(GetByINode(node));
  }

  public void Remove(Node node)
  {
    if (node != null)
    {
      // remove all the edges connected to this node
      // Reverse iterate so we can savely remove items from the list while doing the iteration
      for (int i = node.connections.Count - 1; i >= 0; i--)
      {
        Edge edge = node.connections[i];
        // remove the edge in nodes connections
        edge.displayObject.connections.Remove(edge);
        edge.displaySubject.connections.Remove(edge);

        edgeList.Remove(edge);
        Destroy(edge.gameObject);
      }

      // Destoy the node
      nodeList.Remove(node);
      Destroy(node.gameObject);
    }
  }

  public void Remove()
  {
    if (boundingSphere != null) Destroy(boundingSphere.gameObject);
    if (gameObject != null) Destroy(gameObject);
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
      foreach (Graph graph in parentGraph.subGraphs)
      {
        if (graph != null && graph.creationQuery == creationQuery) graph.Remove();
      }
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

}
