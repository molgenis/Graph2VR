using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VDS.RDF;
using VDS.RDF.Query;
using Dweiss;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Patterns;

public class IGraphBasedGraph : MonoBehaviour
{
    public static IGraphBasedGraph instance;
    public Graph graph;
    public string BaseURI = "http://dbpedia.org";
    public GameObject edgePrefab;
    public GameObject nodePrefab;
    public Canvas menu;

    public HashSet<Triple> triples = new HashSet<Triple>();
    public List<Edge> edgeList = new List<Edge>();
    public List<Node> nodeList = new List<Node>();

    public List<string> translatablePredicates = new List<string>();

    private void Awake()
    {
        instance = this;
    }

    public void SendQuery(string query)
    {
        graph.Clear();
        // load sparql query
        SparqlQueryParser parser = new SparqlQueryParser();
        SparqlQuery sparqlQuery = parser.ParseFromString(query);

        // load pattern
        GraphPattern pattern = sparqlQuery.RootGraphPattern;
        List<ITriplePattern> triplePattern = pattern.TriplePatterns;

        // Execute query
        SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new System.Uri(Settings.Instance.SparqlEndpoint), BaseURI);
        IGraph iGraph = endpoint.QueryWithResultGraph(query);

        foreach(INode node in iGraph.Nodes) {
            Node n = graph.CreateNode(node.ToString(), node);
        }
        
        foreach (Triple triple in iGraph.Triples) {
            Edge e = graph.CreateEdge(triple.Subject, triple.Predicate, triple.Object);
        }

        graph.positionCalculator.SetInitial();
    }
}
