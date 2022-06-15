using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Dweiss;
using UnityEngine;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Patterns;

public class QueryService : MonoBehaviour
{
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
    SwitchEndpoint();
  }

  public void SwitchEndpoint()
  {
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

  public void ExecuteQuery(string query, GraphCallback queryCallback)
  {
    try
    {
      Debug.Log("ExecuteQuery: " + query);
      endPoint.QueryWithResultGraph(query, queryCallback, state: null);
    }
    catch (RdfQueryException error)
    {
      Debug.Log("No database connection found");
      Debug.Log(error);
    }
  }
  public void ExpandGraph(Node node, string uri, bool isOutgoingLink, GraphCallback queryCallback)
  {
    string query = GetExpandGraphQuery(node, uri, isOutgoingLink);
    Debug.Log("ExpandGraph: "+ query);
    endPoint.QueryWithResultGraph(query, queryCallback, state: null);
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
                ?object rdfs:label ?objectlabel .
                ?object a ?type .
            }} where {{
                <{nodeUriString}> <{uri}> ?object .
                OPTIONAL {{
                    ?object rdfs:label ?objectlabel .
                    FILTER(LANG(?objectlabel) = '' || LANGMATCHES(LANG(?objectlabel), '{Main.instance.languageCode}'))
                }}
                OPTIONAL {{
                  ?object a ?type .
                  FILTER(?type = owl:Thing || ?type = owl:Class || ?type = rdfs:subClassOf || ?type = rdf:Property)
                }}
            }} LIMIT " + queryLimit;
    }
    else
    {
      return $@"
            {PREFIXES}
            construct {{
                ?subject <{uri}> <{nodeUriString}> .
                ?subject rdfs:label ?subjectlabel .
                ?subject a ?type .
            }} where {{
                ?subject <{uri}> <{nodeUriString}>
                OPTIONAL {{
                    ?subject rdfs:label ?subjectlabel .
                    FILTER(LANG(?subjectlabel) = '' || LANGMATCHES(LANG(?subjectlabel), '{Main.instance.languageCode}'))
                }}
                OPTIONAL {{
                  ?subject a ?type .
                  FILTER(?type = owl:Thing || ?type = owl:Class || ?type = rdfs:subClassOf || ?type = rdf:Property)
                }}
            }}  LIMIT " + queryLimit;
    }
  }

  public void QueryByTriples(string triples, GraphCallback queryCallback)
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
      ExecuteQuery(query, queryCallback);
    }
    else
    {
      Debug.Log("Please use a Construct query");
    }
  }

  public void GetOutgoingPredicats(string URI, SparqlResultsCallback sparqlResultsCallback)
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
    endPoint.QueryWithResultSet(query, sparqlResultsCallback, state: null);
  }

  public void GetIncomingPredicats(string URI, SparqlResultsCallback sparqlResultsCallback)
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
    endPoint.QueryWithResultSet(query, sparqlResultsCallback, state: null);
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

  public void QuerySimilarPatternsMultipleLayers(string triples, OrderedDictionary orderByList, Action<SparqlResultSet, string> callback)
  {
    // TODO: make sure 'orderByList' do still exist
    string order = GetOrderByString(orderByList);
    string query = $@"
      {PREFIXES}
      select distinct * where {{
        {triples}
      }} {order} LIMIT {queryLimit}";

    endPoint.QueryWithResultSet(query, (SparqlResultSet results, object state) =>
    {
      callback(results, query);
    }, null);
  }


  public void AutocompleteSearch(string searchterm, SparqlResultsCallback callback)
  {
    if (searchterm.Length > 3)
    {
      /*string query = $@"
      {PREFIXES}
      select ?entity ?name (COUNT(?x) AS ?score) 
      where {{
      ?x(^(<>| !<>) | rdfs:label | skos:altLabel) ?entity.
      BIND(STR(?entity) AS ?name).
      FILTER REGEX(?name, '{searchterm}')
      }}
      GROUP BY ?entity ?name ORDER BY DESC(?score) LIMIT 4";*/
      string query = $@"
      {PREFIXES}
      select distinct ?uri ?name 
      where {{
      ?uri(^(<>| !<>) | rdfs:label | skos:altLabel) ?entity.
      BIND(STR(?entity) AS ?name).
      FILTER REGEX(?name, '{searchterm}')
      }}
      LIMIT 5";
      endPoint.QueryWithResultSet(query, callback, state: null);
    }
    else
    {
      callback(null, null);
    }
  }

  private static string GetOrderByString(OrderedDictionary orderByList)
  {
    if (orderByList.Count > 0)
    {
      string result = "Order By ";
      foreach (DictionaryEntry order in orderByList)
      {
        result += $"{order.Value}({order.Key}) ";
      }
      return result;
    }
    else
    {
      return "";
    }
  }

  private SparqlRemoteEndpoint GetEndPoint()
  {
    return new SparqlRemoteEndpoint(new Uri(Settings.Instance.SparqlEndpoint), Settings.Instance.BaseURI);
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
