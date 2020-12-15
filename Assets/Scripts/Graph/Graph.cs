using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VDS.RDF;
using VDS.RDF.Query;


public class Graph : MonoBehaviour
{
    public static Graph instance;
    public string SPARQLEndpoint = "http://dbpedia.org/sparql";
    public string BaseURI = "http://dbpedia.org";
    public GameObject edgePrefab;
    public GameObject nodePrefab;
    public Canvas menu;

    public List<Triple> triples = new List<Triple>();
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

    public void SendQuery(string query)
    {
        Clear();
        SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new System.Uri(SPARQLEndpoint), BaseURI);
        lastResults = endpoint.QueryWithResultSet(query);

        // Fill triples list 
        foreach (SparqlResult result in lastResults) {
            result.TryGetValue("s", out INode s);
            result.TryGetValue("p", out INode p);
            result.TryGetValue("o", out INode o);

            Triple triple = new Triple();
            if (s != null) triple.Subject = s.ToString();
            if (p != null) triple.Predicate = p.ToString();
            if (o != null) triple.Object = o.ToString();
            triples.Add(triple);
        }

        // Create all Subject / Object nodes
        // NOTE: this can be optimised substantially if large sets take long to load
        foreach (Triple triple in triples) {
            // Drop alternate languages
            // TODO: write a more robust implementation
            if (translatablePredicates.Contains(triple.Predicate)) {
                if(!triple.Object.EndsWith("@"+ Main.instance.languageCode)) {
                    continue;
                } else {
                    triple.Object = triple.Object.Substring(0, triple.Object.Length - 3); // remove the language code
                }
            }

            // This is probably a label?
            string label = "";
            if (triple.Predicate.EndsWith("#label")) {
                label = triple.Object;
            }

            // Find or Create a subject node
            Node subjectNode = nodeList.Find(node => node.uri == triple.Subject && node.type == Node.Type.Subject);
            if (subjectNode == null) {
                subjectNode = CreateNode(triple.Subject, Node.Type.Subject);
                if (label != "") {
                    // We have a label, lets use it
                    subjectNode.SetLabel(label);
                }
                nodeList.Add(subjectNode);
            }

            // Always create a Object node, i dont think they need to be made unique?
            Node objectNode = null;
            if (label == "") { // NOTE: Dont create a label node here
                objectNode = CreateNode(triple.Object, Node.Type.Object);
                nodeList.Add(objectNode);
            } else {
                // We dont need to create a edge if this is a label node
                continue;
            }

            // Find or Create a edge
            Edge predicateEdge = edgeList.Find(edge => edge.from == subjectNode && edge.to == objectNode);
            if (predicateEdge == null) {
                // Dont create a label node
                predicateEdge = CreateEdge(subjectNode, triple.Predicate, objectNode);
                edgeList.Add(predicateEdge);
            }

            // Add known connections to node's and edge's
            if (subjectNode != null) {
                if(predicateEdge != null) subjectNode.connectedEdges.Add(predicateEdge);
                if (objectNode != null) subjectNode.connectedNodes.Add(objectNode);
            }
            if (objectNode != null) {
                if (predicateEdge != null) objectNode.connectedEdges.Add(predicateEdge);
                if (subjectNode != null) objectNode.connectedNodes.Add(subjectNode);
            }
        }
        positionCalculator.SetInitial();
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
    }

    private void Awake()
    {
        instance = this;
    }

    private Edge CreateEdge(Node from, string uri,  Node to)
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
        return edge;
    }

    private Node CreateNode(string value, Node.Type type)
    {
        GameObject clone = Instantiate<GameObject>(nodePrefab);
        clone.transform.SetParent(transform);
        clone.transform.localPosition = Vector3.zero;
        clone.transform.localRotation = Quaternion.identity;
        clone.transform.localScale = Vector3.one * 0.3f;
        clone.GetComponent<NodeInteraction>().menu = menu;
        Node node = clone.AddComponent<Node>();
        node.SetValue(value);
        node.type = type;
        return node;
    }
}
