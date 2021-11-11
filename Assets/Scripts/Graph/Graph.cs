using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VDS.RDF;
using VDS.RDF.Query;
using Dweiss;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Patterns;
using System;
using System.Threading;

public class Graph : MonoBehaviour
{
    public BaseLayoutAlgorithm layout = null;
    public Color defaultNodeColor;
    public Color defaultEdgeColor;
    public Color edgeSelectedColor;
    public Color edgeHoverColor;
    public Color edgeGrabbedColor;


    public Color uriNodeColor;
    public Color literalNodeColor;
    public Color variableNodeColor;
    public Color blankNodeColor;

    public static Graph instance;
    public string BaseURI = "https://github.com/PjotrSvetachov/GraphVR/example-graph";
    public GameObject edgePrefab;
    public GameObject nodePrefab;
    public Canvas menu;

    public HashSet<Triple> triples = new HashSet<Triple>();
    public List<Edge> edgeList = new List<Edge>();
    public List<Node> nodeList = new List<Node>();

    public List<string> translatablePredicates = new List<string>();

    private SparqlResultSet lastResults = null;
    IGraph currentGraph = null;

    private SparqlRemoteEndpoint endpoint;

    public VariableNameManager variableNameManager;

    // FIXME: don't store selection in string, it will not reflect changes made after selections!
    // Change to VDS.RDF.Triple
    public List<Edge> selection = new List<Edge>();

    public void QuerySimilarPatterns()
    {
        string triples = "";
        foreach (Edge edge in selection)
        {
            triples += edge.GetQueryString();
        }

        string query = $@"
            construct {{
                {triples} 
            }} where {{
                {triples} 
            }}";

        Debug.Log(query);

        endpoint.QueryWithResultGraph(query, (graph, state) =>
        {
            if (state == null)
            {
                Debug.Log("All good");
                Debug.Log(query);
                Debug.Log(graph);
                Debug.Log(state);
            }
            else
            {
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
        SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new System.Uri(Settings.Instance.SparqlEndpoint), BaseURI);
        lastResults = endpoint.QueryWithResultSet(
            "select distinct ?s where { ?s ?p ?o } LIMIT 10"
            );

        List<string> results = new List<string>();
        // Fill triples list 
        foreach (SparqlResult result in lastResults)
        {
            result.TryGetValue("s", out INode s);
            result.TryGetValue("p", out INode p);
            result.TryGetValue("o", out INode o);

            if (s != null)
            {
                results.Add(s.ToString());
            }
        }

        return results;
    }

    //To expand the graph, we want to know, which outgoing predicates we have for the given Node and how many Nodes are connected for each of the predicates.
    public Dictionary<string, Tuple<string, int>> GetOutgoingPredicats(string URI)
    {
        string query = "";
        if (URI == "") return null;
        try
        {
            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new System.Uri(Settings.Instance.SparqlEndpoint), BaseURI);
            query = "select distinct ?p (STR(COUNT(?o)) AS ?count) STR(?label) AS ?label where { <" + URI + "> ?p ?o . OPTIONAL { ?p rdfs:label ?label } FILTER(LANG(?label) = '' || LANGMATCHES(LANG(?label), '" + Main.instance.languageCode + "')) } LIMIT 100";
            lastResults = endpoint.QueryWithResultSet(query);

            Dictionary<string, Tuple<string, int>> results = new Dictionary<string, Tuple<string, int>>();
            // Fill triples list 
            foreach (SparqlResult result in lastResults)
            {
                //Debug.Log(result);
                result.TryGetValue("p", out INode p);
                result.TryGetValue("count", out INode count);
                result.TryGetValue("label", out INode labelNode);

                string label = "";
                if (labelNode != null)
                {
                    label = labelNode.ToString();
                }
                if (p != null)
                {
                    if (!results.ContainsKey(p.ToString()))
                    {
                        results.Add(p.ToString(), new Tuple<string, int>(label, int.Parse(count.ToString())));
                    }
                    else
                    {
                        if (!results[p.ToString()].Item1.Contains("@" + Main.instance.languageCode))
                        {
                            results[p.ToString()] = new Tuple<string, int>(label, int.Parse(count.ToString()));
                        }
                    }
                }
            }
            return results;
        }
        catch (Exception e)
        {
            Debug.Log("GetOutgoingPredicats error: " + e.Message);
            Debug.Log("URI: " + URI);
            Debug.Log(query);
            Debug.Log(lastResults);

        }
        return null;
    }

    //Sometimes not only the outgoing predicates are important, but also the incoming ones.
    public Dictionary<string, Tuple<string, int>> GetIncomingPredicats(string URI)
    {
        string query = "";
        if (URI == "") return null;
        try
        {

            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new System.Uri(Settings.Instance.SparqlEndpoint), BaseURI);
            query = "select distinct ?p (STR(COUNT(?s)) AS ?count) STR(?label) AS ?label where { ?s ?p <" + URI + "> . OPTIONAL { ?p rdfs:label ?label } FILTER(LANG(?label) = '' || LANGMATCHES(LANG(?label), '" + Main.instance.languageCode + "')) } LIMIT 100";
            lastResults = endpoint.QueryWithResultSet(query);
            Dictionary<string, Tuple<string, int>> results = new Dictionary<string, Tuple<string, int>>();
            // Fill triples list 
            foreach (SparqlResult result in lastResults)
            {
                //Debug.Log(result);
                result.TryGetValue("p", out INode p);
                result.TryGetValue("count", out INode count);
                result.TryGetValue("label", out INode labelNode);
                string label = "";
                if (labelNode != null)
                {
                    label = labelNode.ToString();
                }

                // NOTE: duplicate code, create function?
                if (p != null)
                {
                    if (!results.ContainsKey(p.ToString()))
                    {
                        results.Add(p.ToString(), new Tuple<string, int>(label, int.Parse(count.ToString())));
                    }
                    else
                    {
                        if (!results[p.ToString()].Item1.Contains("@" + Main.instance.languageCode))
                        {
                            results[p.ToString()] = new Tuple<string, int>(label, int.Parse(count.ToString()));
                        }
                    }
                }
            }

            return results;
        }
        catch (Exception e)
        {
            Debug.Log("GetIncomingPredicats error: " + e.Message);
            Debug.Log("URI: " + URI);
            Debug.Log(query);
            Debug.Log(lastResults);

        }
        return null;

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
        List<Triple> tmpList = new List<Triple>();
        IEnumerable<Triple> objects = currentGraph.GetTriplesWithObject(node.graphNode);
        IEnumerable<Triple> subjects = currentGraph.GetTriplesWithSubject(node.graphNode);

        foreach (Triple triple in objects)
        {
            tmpList.Add(triple);
        }

        foreach (Triple triple in subjects)
        {
            tmpList.Add(triple);
        }

        foreach (Triple triple in tmpList)
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
        string query = "";
        if (isOutgoingLink)
        {
            // Select with label
            query = $@"
                prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                construct {{
                    <{node.GetURIAsString()}> <{uri}> ?object .
                    ?object rdfs:label ?objectlabel
                }} where {{
                    <{node.GetURIAsString()}> <{uri}> ?object .
                    OPTIONAL {{
                        ?object rdfs:label ?objectlabel .
                        FILTER(LANG(?objectlabel) = '' || LANGMATCHES(LANG(?objectlabel), '{Main.instance.languageCode}'))
                    }}
                }} LIMIT 20";
        }
        else
        {
            query = $@"
            construct {{
                ?subject <{uri}> <{node.GetURIAsString()}>
            }} where {{
                ?subject <{uri}> <{node.GetURIAsString()}>
            }}  LIMIT 20";
        }
        // Execute query
        endpoint.QueryWithResultGraph(query, (graph, state) =>
        {
            if (state != null)
            {
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

    public string GetShortName(string uri)
    {
        //Get label?

        //Qname
        if (currentGraph.NamespaceMap.ReduceToQName(uri, out string qname))
        {
            return qname;
        }
        return "";
    }

    private string CleanInfo(string str)
    {
        // TODO: do we need: return str.TrimStart('<', '"').TrimEnd('>', '"');
        return str.TrimStart('<').TrimEnd('>');
    }

    public void SendQuery(string query)
    {
        Clear();
        // load sparql query
        SparqlQueryParser parser = new SparqlQueryParser();
        SparqlQuery sparqlQuery = parser.ParseFromString(query);

        // load pattern
        GraphPattern graphPattern = sparqlQuery.RootGraphPattern;
        endpoint = new SparqlRemoteEndpoint(new System.Uri(Settings.Instance.SparqlEndpoint), BaseURI);

        // Execute query
        if (sparqlQuery.QueryType == SparqlQueryType.Construct)
        {
            currentGraph = endpoint.QueryWithResultGraph(query);
            AddDefaultNameSpaces();
            BuildByIGraph(currentGraph);
            currentGraph.TripleAsserted += CurrentGraph_TripleAsserted;
            currentGraph.TripleRetracted += CurrentGraph_TripleRetracted;
        }
        else
        {
            Debug.Log("Please use a Construct query");
        }
    }

    private int NumTriples(IEnumerable<Triple> triples)
    {
        int count = 0;

        foreach (Triple triple in triples)
        {
            count++;
        }

        return count;
    }

    private void CurrentGraph_TripleRetracted(object sender, TripleEventArgs args)
    {
        // Object and subject will get removed when we only have one triple. edge will also get removed then and only then.
        // This event is raised after deletion so we need to see if the object/subject is deleted in the resulting graph
        if (NumTriples(currentGraph.GetTriples(args.Triple.Object)) == 0)
        {
            Remove(nodeList.Find(graficalNode => graficalNode.graphNode.Equals(args.Triple.Object)));
        }
        if (NumTriples(currentGraph.GetTriples(args.Triple.Subject)) == 0)
        {
            Remove(nodeList.Find(graficalNode => graficalNode.graphNode.Equals(args.Triple.Subject)));
        }

    }

    private void CurrentGraph_TripleAsserted(object sender, TripleEventArgs args)
    {
        // Add nodes
        if (nodeList != null && !nodeList.Find(graficalNode => graficalNode.graphNode.Equals(args.Triple.Object)))
        {
            Node n = CreateNode(args.Triple.Object.ToString(), args.Triple.Object);
        }
        if (nodeList != null && !nodeList.Find(graficalNode => graficalNode.graphNode.Equals(args.Triple.Subject)))
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
        currentGraph.NamespaceMap.AddNamespace("rdf", new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#%22"));
        currentGraph.NamespaceMap.AddNamespace("owl", new Uri("http://www.w3.org/2002/07/owl#"));

        // For nice demo's
        currentGraph.NamespaceMap.AddNamespace("dbpedia", new Uri("http://dbpedia.org/resource/"));
        currentGraph.NamespaceMap.AddNamespace("dbpedia/ontology", new Uri("http://dbpedia.org/ontology/"));
    }

    private void BuildByIGraph(IGraph iGraph)
    {
        // Remove all removed edges and nodes
        List<Node> nodesToRemove = new List<Node>();
        foreach (Node node in nodeList)
        {
            INode iNode = node.graphNode;
            // is the node in the current graph?
            bool found = false;

            foreach (INode currentNode in iGraph.Nodes)
            {
                if (iNode.Equals(currentNode))
                {
                    found = true;
                }
            }

            if (found == false)
            {
                //remove me
                nodesToRemove.Add(node);
            }
        }
        Remove(nodesToRemove);

        // Add nodes
        foreach (INode node in iGraph.Nodes)
        {
            if (nodeList != null && !nodeList.Find(graficalNode => graficalNode.graphNode.Equals(node)))
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

    public void Clear()
    {
        // destroy all stuff
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
        instance = this;
        variableNameManager = new VariableNameManager();
    }

    public Edge CreateEdge(Node from, string uri, Node to)
    {
        GameObject clone = Instantiate<GameObject>(edgePrefab);
        clone.transform.SetParent(transform);
        clone.transform.localPosition = Vector3.zero;
        clone.transform.localRotation = Quaternion.identity;
        clone.transform.localScale = Vector3.one;
        Edge edge = clone.AddComponent<Edge>();
        edge.uri = uri;
        edge.displaySubject = from;
        edge.displayObject = to;
        edgeList.Add(edge);

        edge.selectedColor = edgeSelectedColor;
        edge.hoverColor = edgeHoverColor;
        edge.grabbedColor = edgeGrabbedColor;
        edge.SetDefaultColor(defaultEdgeColor);

        return edge;
    }

    public Edge CreateEdge(INode from, INode uri, INode to)
    {
        GameObject clone = Instantiate<GameObject>(edgePrefab);
        clone.transform.SetParent(transform);
        clone.transform.localPosition = Vector3.zero;
        clone.transform.localRotation = Quaternion.identity;
        clone.transform.localScale = Vector3.one;
        Edge edge = clone.AddComponent<Edge>();

        Node fromNode = GetByINode(from);
        Node toNode = GetByINode(to);
        if (fromNode == null || toNode == null)
        {
            Debug.Log("The Subject and Object needs to be defined to create a edge");
            return null;
        }

        edge.uri = uri.ToString();
        edge.graphPredicate = uri;
        edge.graphSubject = from;
        edge.graphObject = to;
        edge.displaySubject = fromNode;
        edge.displayObject = toNode;

        edge.selectedColor = edgeSelectedColor;
        edge.hoverColor = edgeHoverColor;
        edge.grabbedColor = edgeGrabbedColor;
        edge.SetDefaultColor(defaultEdgeColor);
        edgeList.Add(edge);
        return edge;
    }

    public Node CreateNode(string value)
    {
        GameObject clone = Instantiate<GameObject>(nodePrefab);
        clone.transform.SetParent(transform);
        clone.transform.localPosition = UnityEngine.Random.insideUnitSphere * 3f;
        clone.transform.localRotation = Quaternion.identity;
        clone.transform.localScale = Vector3.one * 0.05f;
        Node node = clone.AddComponent<Node>();
        node.SetURI(value);
        node.SetLabel(value);
        nodeList.Add(node);
        return node;
    }

    public Node CreateNode(string value, INode iNode)
    {
        Node node = CreateNode(value);
        node.graphNode = iNode;

        switch (iNode.NodeType)
        {
            case NodeType.Variable:
                node.SetDefaultColor(variableNodeColor);
                break;
            case NodeType.Blank:
                node.SetURI("");
                node.SetDefaultColor(blankNodeColor);
                break;
            case NodeType.Literal:
                node.SetLabel(((ILiteralNode)iNode).Value);
                node.SetURI("");
                node.SetDefaultColor(literalNodeColor);
                break;
            case NodeType.Uri:
                // TODO: this should work?
                node.SetURI(((IUriNode)iNode).Uri.ToString());
                //node.RequestLabel(endpoint);
                node.SetDefaultColor(uriNodeColor);
                break;
                // etc.
        }

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
        node.SetURI(value);
        node.SetLabel(value);
        nodeList.Add(node);
        node.SetDefaultColor(variableNodeColor);
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
            Edge e = edgeList.Find((Edge edge) => edge.graphObject.Equals(node.graphNode));
            Remove(e);
            e = edgeList.Find((Edge edge) => edge.graphSubject.Equals(node.graphNode));
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
}
