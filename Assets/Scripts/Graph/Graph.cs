using System;
using System.Collections.Generic;
using System.Linq;
using Dweiss;
using UnityEngine;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Patterns;

public class Graph : MonoBehaviour
{
    public BaseLayoutAlgorithm layout = null;
    public BoundingSphere boundingSphere;
    public string BaseURI = "http://dbpedia.org"; //"https://github.com/PjotrSvetachov/GraphVR/example-graph";
    public GameObject edgePrefab;
    public GameObject nodePrefab;
    public Canvas menu;
    public int expandGraphAddLimit = 100;
    public HashSet<Triple> triples = new HashSet<Triple>();
    public List<Edge> edgeList = new List<Edge>();
    public List<Node> nodeList = new List<Node>();
    public List<string> translatablePredicates = new List<string>();
    private SparqlResultSet lastResults = null;
    IGraph currentGraph = null;
    private SparqlRemoteEndpoint endpoint;
    public VariableNameManager variableNameManager;
    public List<Edge> selection = new List<Edge>();


    public List<Graph> subGraphs = new List<Graph>();
    public Graph parentGraph = null;
    public string creationQuery = "";

    public Graph QuerySimilarWithTriples(string triples, Vector3 position, Quaternion rotation)
    {
        string query = $@"
            construct {{
                {triples} 
            }} where {{
                {triples} 
            }} LIMIT " + expandGraphAddLimit;

        Graph newGraph = Main.instance.CreateGraph();
        newGraph.parentGraph = this;
        subGraphs.Add(newGraph);
        newGraph.transform.position = position;
        newGraph.transform.rotation = rotation;
        newGraph.SendQuery(query);
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

        SparqlRemoteEndpoint endpoint = GetEndPoint();
        string query = "select distinct * where { " + triples + " } LIMIT 50";
        lastResults = endpoint.QueryWithResultSet(query);

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
                CreateNewGraph(newGraph, query, offset, rotation);
            }
        }
    }

    private void CreateNewGraph(Graph newGraph, string query, Vector3 offset, Quaternion rotation)
    {
        newGraph.creationQuery = query;
        offset += rotation * new Vector3(0, 0, 0.5f);
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
    private SparqlRemoteEndpoint GetEndPoint()
    {
        return new SparqlRemoteEndpoint(new System.Uri(Settings.Instance.SparqlEndpoint), BaseURI);
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

    public Dictionary<string, Tuple<string, int>> GetOutgoingPredicats(string URI)
    {
        if (URI == "") return null;
        try
        {
            string query = "prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>" +
                    "select distinct ?p (STR(COUNT(?o)) AS ?count) STR(?label) AS ?label where { <" +
                     URI + "> ?p ?o . OPTIONAL { ?p rdfs:label ?label } FILTER(LANG(?label) = '' || LANGMATCHES(LANG(?label), '" +
                     Main.instance.languageCode + "')) } ORDER BY ?label ?p LIMIT 100";
            lastResults = endpoint.QueryWithResultSet(query);
            return GetPredicatsList();
        }
        catch (Exception e)
        {
            Debug.Log("GetOutgoingPredicats error: " + e.Message);
            Debug.Log("URI: " + URI);
            Debug.Log(lastResults);
        }
        return null;
    }

    private Dictionary<string, Tuple<string, int>> GetPredicatsList()
    {
        Dictionary<string, Tuple<string, int>> results = new Dictionary<string, Tuple<string, int>>();
        foreach (SparqlResult result in lastResults)
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

    public Dictionary<string, Tuple<string, int>> GetIncomingPredicats(string URI)
    {
        if (URI == "") return null;
        try
        {
            string query = "select distinct ?p (STR(COUNT(?s)) AS ?count) STR(?label) AS ?label where { ?s ?p <" + URI +
            "> . OPTIONAL { ?p rdfs:label ?label } FILTER(LANG(?label) = '' || LANGMATCHES(LANG(?label), '" +
            Main.instance.languageCode + "')) } ORDER BY ?label ?p LIMIT 100";
            lastResults = endpoint.QueryWithResultSet(query);
            return GetPredicatsList();
        }
        catch (Exception e)
        {
            Debug.Log("GetIncomingPredicats error: " + e.Message);
            Debug.Log("URI: " + URI);
            Debug.Log(lastResults);
            return null;
        }
    }
    public Dictionary<string, Tuple<string, int>> GetPredicats(string query)
    {
        SparqlRemoteEndpoint endpoint = GetEndPoint();
        lastResults = endpoint.QueryWithResultSet(query);
        return GetPredicatsList();
    }

    public void GetDescriptionAsync(string URI, GraphCallback callback)
    {
        string query = "describe <" + URI + ">";
        endpoint.QueryWithResultGraph(query, callback, null);
    }

    public void CollapseGraph(Node node)
    {
        CollapseIncomingGraph(node);
        CollapseOutgoingGraph(node);
    }

    public void RemoveNode(Node node)
    {
        List<Triple> triples = new List<Triple>();
        IEnumerable<Triple> objects = currentGraph.GetTriplesWithObject(node.graphNode);
        IEnumerable<Triple> subjects = currentGraph.GetTriplesWithSubject(node.graphNode);

        foreach (Triple triple in objects)
        {
            triples.Add(triple);
        }

        foreach (Triple triple in subjects)
        {
            triples.Add(triple);
        }

        foreach (Triple triple in triples)
        {
            currentGraph.Retract(triple);
        }
    }

    public void CollapseIncomingGraph(Node node)
    {
        string query = $@"
                prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                construct {{
                    ?s ?p2 <{node.GetURIAsString()}> .
                }} where {{
                    ?s ?p2 <{node.GetURIAsString()}> .
                    FILTER NOT EXISTS {{
                        {{
                            ?s ?p3 ?o3 .
                            Filter(?o3 != <{node.GetURIAsString()}>)
                        }} UNION {{
                            ?o4 ?p4 ?s .
                            Filter(?o4 != <{node.GetURIAsString()}>)
                        }}
                    }}
                }}";

        VDS.RDF.Graph results = (VDS.RDF.Graph)currentGraph.ExecuteQuery(query);

        foreach (Triple triple in results.Triples)
        {
            currentGraph.Retract(triple);
        }
    }

    public void CollapseOutgoingGraph(Node node)
    {
        string query = $@"
                prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                construct {{
                    <{node.GetURIAsString()}> ?p ?o .
                }} where {{
                      <{node.GetURIAsString()}> ?p ?o .
                      FILTER NOT EXISTS {{
                          {{
                              ?o2 ?p2 ?o .
                              Filter(?o2 != <{node.GetURIAsString()}>)
                          }} UNION {{
                              ?o ?p3 ?o3 .
                              Filter(?o3 != <{node.GetURIAsString()}>)
                          }}
                      }}
                }}";

        VDS.RDF.Graph results = (VDS.RDF.Graph)currentGraph.ExecuteQuery(query);

        foreach (Triple triple in results.Triples)
        {
            currentGraph.Retract(triple);
        }
    }

    public void ExpandGraph(Node node, string uri, bool isOutgoingLink)
    {
        string query = GetExpandGraphQuery(node, uri, isOutgoingLink);
        endpoint.QueryWithResultGraph(query, (graph, state) =>
        {
            if (state != null)
            {
                //Todo: Fix this error - it still occurs sometimes (graph = null)
                Debug.Log("There may be an error");
                Debug.Log(query);
                Debug.Log(graph);
                Debug.Log(state);
                Debug.Log(((AsyncError)state).Error);
            }

            // To draw new elements to unity we need to be on the main Thread
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                currentGraph.Merge(graph);
            });
        }, null);
    }

    private string GetExpandGraphQuery(Node node, string uri, bool isOutgoingLink)
    {
        string nodeUriString = node.GetURIAsString();
        if (isOutgoingLink)
        {
            // Select with label
            return $@"
            prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>
            construct {{
                <{nodeUriString}> <{uri}> ?object .
                ?object rdfs:label ?objectlabel
            }} where {{
                <{nodeUriString}> <{uri}> ?object .
                OPTIONAL {{
                    ?object rdfs:label ?objectlabel .
                    FILTER(LANG(?objectlabel) = '' || LANGMATCHES(LANG(?objectlabel), '{Main.instance.languageCode}'))
                }}
            }} LIMIT " + expandGraphAddLimit;
        }
        else
        {
            return $@"
            construct {{
                ?subject <{uri}> <{nodeUriString}>
            }} where {{
                ?subject <{uri}> <{nodeUriString}>
            }}  LIMIT " + expandGraphAddLimit;
        }
    }

    // Return a short name if possible
    public string GetShortName(string uri)
    {
        if (currentGraph.NamespaceMap.ReduceToQName(uri, out string qName))
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

    public void SendQuery(string query)
    {
        Clear();

        if (IsConstructSparqlQuery(query))
        {
            ExecuteQuery(query);
        }
        else
        {
            Debug.Log("Please use a Construct query");
        }

        DestroyGraphWithoutTriples();

    }

    private void DestroyGraphWithoutTriples()
    {
        if (currentGraph == null || currentGraph.Triples == null || currentGraph.Triples.Count == 0)
        {
            Destroy(gameObject);
        }
    }

    private void ExecuteQuery(string query)
    {
        try
        {
            currentGraph = endpoint.QueryWithResultGraph(query);
            AddDefaultNameSpaces();
            BuildByIGraph(currentGraph);
            currentGraph.TripleAsserted += CurrentGraph_TripleAsserted;
            currentGraph.TripleRetracted += CurrentGraph_TripleRetracted;
        }
        catch (RdfQueryException error)
        {
            Debug.Log("No database connection found");
            Debug.Log(error);
        }
    }

    private Boolean IsConstructSparqlQuery(string query)
    {
        SparqlQuery sparqlQuery = GetSparqlQuery(query);
        return sparqlQuery != null && sparqlQuery.QueryType == SparqlQueryType.Construct;
    }

    private SparqlQuery GetSparqlQuery(string query)
    {
        try
        {
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery sparqlQuery = null;
            sparqlQuery = parser.ParseFromString(query);

            // load pattern
            GraphPattern graphPattern = sparqlQuery.RootGraphPattern;
            endpoint = GetEndPoint();
            return sparqlQuery;
        }
        catch (RdfParseException error)
        {
            Debug.Log("Error parsing query");
            Debug.Log(error);
            return null;
        }

    }

    private int countTriples(IEnumerable<Triple> triples)
    {
        return triples.Count();
    }

    private void CurrentGraph_TripleRetracted(object sender, TripleEventArgs args)
    {
        // Object and subject will get removed when we only have one triple. edge will also get removed then and only then.
        // This event is raised after deletion so we need to see if the object/subject is deleted in the resulting graph
        if (countTriples(currentGraph.GetTriples(args.Triple.Object)) == 0)
        {
            Remove(nodeList.Find(graphicalNode => graphicalNode.graphNode.Equals(args.Triple.Object)));
        }
        if (countTriples(currentGraph.GetTriples(args.Triple.Subject)) == 0)
        {
            Remove(nodeList.Find(graphicalNode => graphicalNode.graphNode.Equals(args.Triple.Subject)));
        }
    }

    private void CurrentGraph_TripleAsserted(object sender, TripleEventArgs args)
    {
        // Add objects
        if (nodeList != null && !nodeList.Find(graphicalNode => graphicalNode.graphNode.Equals(args.Triple.Object)))
        {
            Node n = CreateNode(args.Triple.Object.ToString(), args.Triple.Object);
        }

        // Add subjects
        if (nodeList != null && !nodeList.Find(graphicalNode => graphicalNode.graphNode.Equals(args.Triple.Subject)))
        {
            Node n = CreateNode(args.Triple.Subject.ToString(), args.Triple.Subject);
        }

        // Add edges
        if (!edgeList.Find(edge => edge.Equals(args.Triple.Subject, args.Triple.Predicate, args.Triple.Object)))
        {
            Edge e = CreateEdge(args.Triple.Subject, args.Triple.Predicate, args.Triple.Object);
        }

        layout.CalculateLayout();
    }

    private void AddDefaultNameSpaces()
    {
        currentGraph.NamespaceMap.AddNamespace("rdf", new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#"));
        currentGraph.NamespaceMap.AddNamespace("owl", new Uri("http://www.w3.org/2002/07/owl#"));

        // For nice demo's
        currentGraph.NamespaceMap.AddNamespace("dbpedia", new Uri("http://dbpedia.org/resource/"));
        currentGraph.NamespaceMap.AddNamespace("dbpedia/ontology", new Uri("http://dbpedia.org/ontology/"));
    }

    private void BuildByIGraph(IGraph iGraph)
    {
        List<Node> nodesToRemove = new List<Node>();
        foreach (Node node in nodeList)
        {
            INode iNode = node.graphNode;
            bool found = FindNodeInGraph(iGraph, iNode);
            if (found == false)
            {
                nodesToRemove.Add(node);
            }
        }
        Remove(nodesToRemove);

        // Add nodes
        foreach (INode node in iGraph.Nodes)
        {
            if (nodeList != null && !nodeList.Find(graphicalNode => graphicalNode.graphNode.Equals(node)))
            {
                Node n = CreateNode(node.ToString(), node);
            }
        }

        // Add edges
        foreach (Triple triple in iGraph.Triples)
        {
            if (!edgeList.Find(edge => edge.Equals(triple.Subject, triple.Predicate, triple.Object)))
            {
                Edge e = CreateEdge(triple.Subject, triple.Predicate, triple.Object);
            }
        }

        // TODO: create resolve function
        layout.CalculateLayout();
    }
    private Boolean FindNodeInGraph(IGraph iGraph, INode iNode)
    {
        foreach (INode currentNode in iGraph.Nodes)
        {
            if (iNode.Equals(currentNode))
            {
                return true;
            }
        }
        return false;
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
        triples.Clear();
    }

    private void Awake()
    {
        variableNameManager = new VariableNameManager();
    }

    public Edge CreateEdge(Node from, string uri, Node to)
    {
        GameObject clone = GetEdgeClone();

        from.AddConnection(to);
        to.AddConnection(from);

        Edge edge = InitializeEdge(clone, uri, from, to);
        edgeList.Add(edge);
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
        fromNode.AddConnection(toNode);
        toNode.AddConnection(fromNode);

        Edge edge = InitializeEdge(clone, uri.ToString(), fromNode, toNode);
        edge.graphPredicate = uri;
        edge.graphSubject = from;
        edge.graphObject = to;

        edgeList.Add(edge);
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

    public Node CreateNode(string value)
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
        return node;
    }

    public Node CreateNode(string value, INode iNode)
    {
        Node node = CreateNode(value);
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

    public void Remove(List<Node> nodes)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            Remove(nodes[i]);
        }
    }

    public void Remove(List<Edge> edges)
    {
        for (int i = 0; i < edges.Count; i++)
        {
            Remove(edges[i]);
        }
    }

    public void Remove(Node node)
    {
        if (node != null)
        {
            // remove edges connected to this node
            Edge e = edgeList.Find((Edge edge) => edge.graphObject == null ? false : edge.graphObject.Equals(node.graphNode));
            Remove(e);
            e = edgeList.Find((Edge edge) => edge.graphSubject == null ? false : edge.graphSubject.Equals(node.graphNode));
            Remove(e);

            // Destoy the node
            nodeList.Remove(node);
            Destroy(node.gameObject);
        }
    }

    public void Remove(Edge edge)
    {
        if (edge != null)
        {
            edgeList.Remove(edge);
            Destroy(edge.gameObject);
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
