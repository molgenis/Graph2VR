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

public class Graph : MonoBehaviour
{
    public static Graph instance;
    public string BaseURI = "http://dbpedia.org";
    public GameObject edgePrefab;
    public GameObject nodePrefab;
    public Canvas menu;

    public HashSet<Triple> triples = new HashSet<Triple>();
    public List<Edge> edgeList = new List<Edge>();
    public List<Node> nodeList = new List<Node>();

    public List<string> translatablePredicates = new List<string>();
    public BasePositionCalculator positionCalculator = null;

    private SparqlResultSet lastResults = null;

    [System.Serializable]
    public class Triple
    {
        public string Subject = null;
        public string Predicate = null;
        public string Object = null;
    }

    // variables for the Fruchterman-Reingold algorithm
    public float Temperature = 0.05f;

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
    public Dictionary<string, int> GetOutgoingPredicats(string URI)
    {
        SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new System.Uri(Settings.Instance.SparqlEndpoint), BaseURI);
        lastResults = endpoint.QueryWithResultSet(
            "select distinct ?p (STR(COUNT(?o)) AS ?count) where { <"+ URI + "> ?p ?o } LIMIT 100"
            );

        Dictionary<string, int> results = new Dictionary<string, int>();
        // Fill triples list 
        foreach (SparqlResult result in lastResults)
        {
            Debug.Log(result);
            result.TryGetValue("p", out INode p);
            result.TryGetValue("count", out INode count);

            if (p != null)
            {
                Debug.Log("Here is what I logged:" + int.Parse(count.ToString()));
                results.Add(p.ToString(), int.Parse(count.ToString()));
            }
        }

        return results;
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
        GraphPattern pattern = sparqlQuery.RootGraphPattern;

        // Execute query
        if (sparqlQuery.QueryType == SparqlQueryType.Construct) {
            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new System.Uri(Settings.Instance.SparqlEndpoint), BaseURI);
            IGraph iGraph = endpoint.QueryWithResultGraph(query);
            BuildByIGraph(iGraph);
        } else {
            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new System.Uri(Settings.Instance.SparqlEndpoint), BaseURI);
            lastResults = endpoint.QueryWithResultSet(query);
            BuildByResultSet(lastResults, pattern);
        }
        positionCalculator.SetInitial();
    }

    private void BuildByIGraph(IGraph iGraph)
    {
        foreach (INode node in iGraph.Nodes) {
            Node n = CreateNode(node.ToString(), node);
        }

        foreach (VDS.RDF.Triple triple in iGraph.Triples) {
            Edge e = CreateEdge(triple.Subject, triple.Predicate, triple.Object);
        }
    }

    private void BuildByResultSet(SparqlResultSet resultSet, GraphPattern pattern)
    {
        List<ITriplePattern> triplePattern = pattern.TriplePatterns;

        // join pattern with query
        foreach (TriplePattern triple in triplePattern) {
            string constantSubject = triple.Subject.VariableName;
            string constantPredicate = triple.Predicate.VariableName;
            string constantObject = triple.Object.VariableName;

            foreach (SparqlResult result in resultSet) {
                Triple t = new Triple();

                if (constantSubject == null) t.Subject = triple.Subject.ToString().TrimStart('<').TrimEnd('>');
                else t.Subject = result.Value(constantSubject).ToString();

                if (constantPredicate == null) t.Predicate = triple.Predicate.ToString().TrimStart('<').TrimEnd('>');
                else t.Predicate = result.Value(constantPredicate).ToString();

                if (constantObject == null) t.Object = triple.Object.ToString().TrimStart('<').TrimEnd('>');
                else t.Object = result.Value(constantObject).ToString();

                triples.Add(t);
            }
        }

        foreach (Triple triple in triples) {

            /*
            // Drop alternate languages
            if (o != null) {
                if (o is ILiteralNode) {
                    ILiteralNode oLiteral = o as ILiteralNode;
                    if (oLiteral.Language.Length == 0 || oLiteral.Language.Equals(Main.instance.languageCode)) {
                        triple.Object = oLiteral.Value;
                    } else {
                        continue;
                    }
                } else {
                    triple.Object = o.ToString();
                }
            }
            if (s != null) triple.Subject = s.ToString();
            if (p != null) triple.Predicate = p.ToString();
            */

            // Create all Subject / Object nodes
            string label = "";
            if (triple.Predicate == "http://www.w3.org/2000/01/rdf-schema#label") {
                //label = triple.Object;
            }

            // Find or Create a subject node
            Node subjectNode = nodeList.Find(node => node.uri == triple.Subject);
            if (subjectNode == null) {
                subjectNode = CreateNode(triple.Subject);
                if (label != "") {
                    // We have a label, lets use it
                    subjectNode.SetLabel(label);
                }
            }

            // Always create a Object node, i dont think they need to be made unique?
            Node objectNode = nodeList.Find(node => node.uri == triple.Object);
            if (label == "") { // NOTE: Dont create a label node here
                if (objectNode == null) {
                    objectNode = CreateNode(triple.Object);
                }
            } else {
                // We dont need to create a edge if this is a label node
                continue;
            }

            // Find or Create a edge
            Edge predicateEdge = edgeList.Find(edge => edge.from == subjectNode && edge.to == objectNode && edge.uri == triple.Predicate);
            if (predicateEdge == null) {
                predicateEdge = CreateEdge(subjectNode, triple.Predicate, objectNode);
            }

            // Add known connections to node's and edge's
            if (subjectNode != null) {
                if (predicateEdge != null) subjectNode.connectedEdges.Add(predicateEdge);
                if (objectNode != null) subjectNode.connectedNodes.Add(objectNode);
            }
            if (objectNode != null) {
                if (predicateEdge != null) objectNode.connectedEdges.Add(predicateEdge);
                if (subjectNode != null) objectNode.connectedNodes.Add(subjectNode);
            }
        }
    }

    public void Clear()
    {
        // destroy all stuff
        for (int i = 0; i < nodeList.Count; i++) {
            Destroy(nodeList[i].gameObject);
        }
        for (int i = 0; i < edgeList.Count; i++) {
            Destroy(edgeList[i].gameObject);
        }
        nodeList.Clear();
        edgeList.Clear();
        triples.Clear();
    }

    private void Awake()
    {
        instance = this;
    }

    public Node GetByINode(INode iNode)
    {
        return nodeList.Find((Node node) => node.iNode.Equals(iNode));
    }

    public Edge CreateEdge(Node from, string uri,  Node to)
    {
        GameObject clone = Instantiate<GameObject>(edgePrefab);
        clone.transform.SetParent(transform);
        clone.transform.localPosition = Vector3.zero;
        clone.transform.localRotation = Quaternion.identity;
        clone.transform.localScale = Vector3.one;
        Edge edge = clone.AddComponent<Edge>();
        edge.uri = uri;
        edge.from = from;
        edge.to = to;
        edgeList.Add(edge);
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
        if(fromNode == null || toNode == null) {
            Debug.Log("The Subject and Object needs to be defined to create a edge");
            return null;
        }

        edge.uri = uri.ToString();
        edge.iNode = uri;
        edge.from = fromNode;
        edge.to = toNode;
        edgeList.Add(edge);
        return edge;
    }

    public Node CreateNode(string value)
    {
        GameObject clone = Instantiate<GameObject>(nodePrefab);
        clone.transform.SetParent(transform);
        clone.transform.localPosition = Vector3.zero;
        clone.transform.localRotation = Quaternion.identity;
        clone.transform.localScale = Vector3.one * 0.3f;
        Node node = clone.AddComponent<Node>();
        node.SetValue(value);
        nodeList.Add(node);
        return node;
    }

    public Node CreateNode(string value, INode iNode)
    {
        Node node = CreateNode(value);
        node.iNode = iNode;
        return node;
    }


    public Node CreateNode(string value, Vector3 position)
    {
        GameObject clone = Instantiate<GameObject>(nodePrefab);
        clone.transform.SetParent(transform);
        clone.transform.position = position;
        clone.transform.localRotation = Quaternion.identity;
        clone.transform.localScale = Vector3.one * 0.3f;
        Node node = clone.AddComponent<Node>();
        node.SetValue(value);
        nodeList.Add(node);
        return node;
    }

    void Update()
    {
        if(Temperature > 0.01f)
        {
            FruchtermanReingoldIteration();
        }
    }

    public float C_CONSTANT = 1.0f;
    public float AREA_CONSTANT = 1.0f;
    //Function for the Fruchterman-Reingold algorithm
    private float Fa(float x)
    {
        return (x*x)/(C_CONSTANT*Mathf.Sqrt(AREA_CONSTANT/nodeList.Count));
    }

    //Function for the Fruchterman-Reingold algorithm
    private float Fr(float x)
    {
        return ((C_CONSTANT * Mathf.Sqrt(AREA_CONSTANT / nodeList.Count))* (C_CONSTANT * Mathf.Sqrt(AREA_CONSTANT / nodeList.Count))) / x;
    }

    // Do one iteration fo the Fruchterman-Reingold algorithm
    // We only use localpositions so the algorithm stays stable when zooming in/out
    public void FruchtermanReingoldIteration()
    {
        // calculate repulsive forces
        foreach(Node node in nodeList)
        {
            node.displacement = Vector3.zero;
            foreach (Node neightbor in nodeList)
            {
                if(node != neightbor)
                {
                    Vector3 delta = node.transform.localPosition - neightbor.transform.localPosition;
                    if (delta.magnitude < 1)
                    {
                        float FrForce = Fr(delta.magnitude);
                        node.displacement += delta.normalized * FrForce;
                    }
                }
            }
        }

        // calculate attractive forces
        foreach(Edge edge in edgeList)
        {
            Vector3 delta = edge.to.transform.localPosition - edge.from.transform.localPosition;
            float FaForce = Fa(delta.magnitude);
            Vector3 normal = delta.normalized;
            edge.to.displacement -= normal * FaForce;
            edge.from.displacement += normal * FaForce;
        }

        // Reposition the nodes, taking ionto account the temperature
        float TotalDisplacement = 0.0f;
        foreach (Node node in nodeList)
        {
            float DisplacementMagitude = node.displacement.magnitude;
            if (DisplacementMagitude > 0.3f)
            {
                TotalDisplacement = Mathf.Max(DisplacementMagitude, TotalDisplacement);
                node.transform.localPosition += (node.displacement / DisplacementMagitude) * Mathf.Min(DisplacementMagitude, Temperature);
            }
        }

        // reduce the temperature
        Temperature -= 0.0001f;
    }
}
