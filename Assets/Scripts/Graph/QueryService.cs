using System;
using System.Collections.Generic;
using System.Linq;
using Dweiss;
using UnityEngine;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Patterns;

public class QueryService : MonoBehaviour
{
  public string BaseURI = "http://dbpedia.org"; //"https://github.com/PjotrSvetachov/GraphVR/example-graph";
  public int queryLimit = 25;
  const string PREFIXES = @"
    prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
    prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>";

  public INamespaceMapper defaultNamespace = new NamespaceMapper(true);

  SparqlRemoteEndpoint endPoint = null;
  private void Awake()
  {
    SetupSingelton();
    AddDefaultNamespaces();
    this.endPoint = GetEndPoint();
  }

  private void AddDefaultNamespaces()
  {
    defaultNamespace.AddNamespace("rdf", new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#"));
    defaultNamespace.AddNamespace("owl", new Uri("http://www.w3.org/2002/07/owl#"));
    // For nice demo's
    defaultNamespace.AddNamespace("dbpedia", new Uri("http://dbpedia.org/resource/"));
    defaultNamespace.AddNamespace("dbpedia/ontology", new Uri("http://dbpedia.org/ontology/"));
  }

  public IGraph ExecuteQuery(string query)
  {
    try
    {
      IGraph graph = endPoint.QueryWithResultGraph(query);
      graph.NamespaceMap.Import(defaultNamespace);
      return graph;
    }
    catch (RdfQueryException error)
    {
      Debug.Log("No database connection found");
      Debug.Log(error);
      return null;
    }
  }

  private string GetExpandGraphQuery(Node node, string uri, bool isOutgoingLink)
  {
    string nodeUriString = node.GetURIAsString();
    if (isOutgoingLink)
    {
      // Select with label
      return $@"
            {PREFIXES}
            construct {{
                <{nodeUriString}> <{uri}> ?object .
                ?object rdfs:label ?objectlabel
            }} where {{
                <{nodeUriString}> <{uri}> ?object .
                OPTIONAL {{
                    ?object rdfs:label ?objectlabel .
                    FILTER(LANG(?objectlabel) = '' || LANGMATCHES(LANG(?objectlabel), '{Main.instance.languageCode}'))
                }}
            }} LIMIT " + queryLimit;
    }
    else
    {
      return $@"
            {PREFIXES}
            construct {{
                ?subject <{uri}> <{nodeUriString}>
            }} where {{
                ?subject <{uri}> <{nodeUriString}>
            }}  LIMIT " + queryLimit;
    }
  }

  public void ExpandGraph(Node node, string uri, bool isOutgoingLink, GraphCallback queryCallback)
  {
    string query = GetExpandGraphQuery(node, uri, isOutgoingLink);
    endPoint.QueryWithResultGraph(query, queryCallback, null);
  }

  public IGraph QueryByTriples(string triples)
  {
    string query = $@"
            {PREFIXES}
            construct {{
                {triples} 
            }} where {{
                {triples} 
            }} LIMIT {queryLimit}";

    if (IsConstructSparqlQuery(query))
    {
      return ExecuteQuery(query);
    }
    else
    {
      Debug.Log("Please use a Construct query");
      return null;
    }
  }

  public IGraph CollapseIncomingGraph(Node node)
  {
    string query = $@"
            {PREFIXES}
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
    return ExecuteQuery(query);
  }

  public IGraph CollapseOutgoingGraph(Node node)
  {
    string query = $@"
            {PREFIXES}
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

    return ExecuteQuery(query);
  }

  public SparqlResultSet GetOutgoingPredicats(string URI)
  {
    string query = $@"
      {PREFIXES}
      select distinct ?p (STR(COUNT(?o)) AS ?count) STR(?label) AS ?label 
      where {{
        <{URI}> ?p ?o .
        OPTIONAL {{
          ?p rdfs:label ?label
        }}
        FILTER(LANG(?label) = '' || LANGMATCHES(LANG(?label), '{Main.instance.languageCode}')) 
      }}
      ORDER BY ?label ?p LIMIT 100";
    return endPoint.QueryWithResultSet(query);
  }

  public SparqlResultSet GetIncomingPredicats(string URI)
  {
    string query = $@"
      {PREFIXES}
      select distinct ?p (STR(COUNT(?s)) AS ?count) STR(?label) AS ?label 
      where {{ 
        ?s ?p <{URI}> . 
        OPTIONAL {{
          ?p rdfs:label ?label
        }}
        FILTER(LANG(?label) = '' || LANGMATCHES(LANG(?label), '{Main.instance.languageCode}')) 
      }} 
      ORDER BY ?label ?p LIMIT 100";
    return endPoint.QueryWithResultSet(query);
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

      GraphPattern graphPattern = sparqlQuery.RootGraphPattern;
      return sparqlQuery;
    }
    catch (RdfParseException error)
    {
      Debug.Log("Error parsing query");
      Debug.Log(error);
      return null;
    }
  }

  public void GetDescriptionAsync(string URI, GraphCallback callback)
  {
    string query = "describe <" + URI + ">";
    endPoint.QueryWithResultGraph(query, callback, null);
  }

  public SparqlResultSet QuerySimilarPatternsMultipleLayers(string triples, List<string> orderByList, out string query)
  {
    // TODO: make sure 'orderByList' do still exist
    string order = GetOrderByString(orderByList);
    query = $@"
      {PREFIXES}
      select distinct * where {{
        {triples}
      }} {order} LIMIT {queryLimit}";
    return endPoint.QueryWithResultSet(query);
  }

  private static string GetOrderByString(List<string> orderByList)
  {
    if (orderByList.Count > 0)
    {
      return orderByList.Aggregate("Order By", (accum, name) => accum += $"DESC({name}) ");
    }
    else
    {
      return "";
    }
  }

  private SparqlRemoteEndpoint GetEndPoint()
  {
    return new SparqlRemoteEndpoint(new Uri(Settings.Instance.SparqlEndpoint), BaseURI);
  }

  #region  Singleton
  public static QueryService _instance;
  public static QueryService Instance { get { return _instance; } }
  private void SetupSingelton()
  {
    if (_instance != null)
    {
      Debug.LogError("Error in settings. Multiple singletons exists: " + _instance.name + " and now " + this.name);
    }
    else
    {
      _instance = this;
    }
  }
  #endregion
}
