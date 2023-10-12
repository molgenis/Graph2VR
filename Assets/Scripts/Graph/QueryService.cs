﻿using Dweiss;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using UnityEngine;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Patterns;

public class QueryService : MonoBehaviour
{
  public int queryLimit = 25;
  public static int searchResultsLimit = 100;

  const string PREFIXES = @"
    prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
    prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>
    prefix skos: <http://www.w3.org/2004/02/skos/core#>";

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

  public void ExpandGraph(Node node, string uri, bool isOutgoingLink, Action<IGraph, IGraph, object> queryCallback)
  {
    string refinmentQuery = GetExpandGraphQuery(node, uri, isOutgoingLink);
    string dataquery = GetSimpleExpandGraphQuery(node, uri, isOutgoingLink);

    endPoint.QueryWithResultGraph(refinmentQuery, (completeGraph, state) =>
    {
      IGraph dataGraph = completeGraph.ExecuteQuery(dataquery) as IGraph;
      queryCallback(dataGraph, completeGraph, state);
    }, state: null);
  }


  private string GetSimpleExpandGraphQuery(Node node, string uri, bool isOutgoingLink)
  {
    string value = node.graph.RealNodeValue(node.graphNode);
    if (isOutgoingLink)
    {

      // Select with label
      return $@"
            {PREFIXES}
            construct {{
                {value} <{uri}> ?object .
            }} where {{
                {value} <{uri}> ?object .
            }} 
            LIMIT {queryLimit}";
    }
    else
    {
      return $@"
            {PREFIXES}
            construct {{
                ?subject <{uri}> {value} .
            }} where {{
                ?subject <{uri}> {value}
            }}  
            LIMIT {queryLimit}";
    }
  }


  private string GetExpandGraphQuery(Node node, string uri, bool isOutgoingLink)
  {
    string value = node.graph.RealNodeValue(node.graphNode);

    if (isOutgoingLink)
    {

      // Select with label
      return $@"
        {PREFIXES}
        construct {{
            {value} <{uri}> ?object .
            ?object ?graph2vrlabel ?label .
            ?object ?graph2vrimage ?image .
            ?object a ?type .
        }} where {{
            {value} <{uri}> ?object .
            {GetOptionalGraphQuery("?object")}
        }} 
        LIMIT {queryLimit}";
    }
    else
    {
      return $@"
        {PREFIXES}
        construct {{
            ?subject <{uri}> {value} .
            ?subject ?graph2vrlabel ?label .
            ?subject ?graph2vrimage ?image .
            ?subject a ?type .
        }} where {{
            ?subject <{uri}> {value}
            {GetOptionalGraphQuery("?subject")}
        }}  
        LIMIT {queryLimit}";
    }
  }

  private string GetOptionalGraphQuery(string variable)
  {
    string imagePredicates = "";

    bool isFirstPredicate = true;
    foreach (string predicate in Settings.Instance.imagePredicates)
    {
      imagePredicates += (isFirstPredicate ? "" : "|") + " <" + predicate + "> ";
      isFirstPredicate = false;
    }

    return $@"
        Optional{{
          Select {variable} <http://graph2vr.org/label> AS ?graph2vrlabel sample(STR(?label)) as ?label
          where {{
            {variable} rdfs:label ?label .
            {LanguageFilterString("?label")}
          }}
        }}

        Optional{{
          Select {variable} <http://graph2vr.org/image> AS ?graph2vrimage sample(?image) as ?image
          where {{
            {variable} ({imagePredicates}) ?image .
            FILTER( strStarts( STR(?image), 'http://' ) || strStarts( STR(?image), 'https://' ) ) .
          }}
        }}

        OPTIONAL {{
          {variable} a ?type .
          FILTER(?type = owl:Thing || ?type = owl:Class || ?type = rdfs:subClassOf || ?type = rdf:Property)
        }}
    ";
  }
  public void ExecuteQuery(string query, Action<IGraph, bool> callback, bool additiveMode = false)
  {
    try
    {
      endPoint.QueryWithResultGraph(query, (IGraph results, object state) =>
      {
        callback(results, additiveMode);
      }, state: null);
    }
    catch (RdfQueryException error)
    {
      Debug.Log("No database connection found");
      Debug.Log(error);
    }
  }

  public void QueryByTriples(string triples, Action<IGraph, bool> callback, bool additiveMode = false)
  {
    string query = $@"
            {PREFIXES}
            construct {{
                {removeOptional(triples)} 
            }} where {{
                {triples} 
            }} LIMIT {queryLimit}";
    if (IsConstructSparqlQuery(query))
    {
      ExecuteQuery(query, callback, additiveMode);
    }
    else
    {
      Debug.Log("Please use a Construct query");
    }
  }

  private string removeOptional(string triples)
  {
    triples = triples.Replace("OPTIONAL {", "");
    triples = triples.Replace("}\n", "");
    return triples;
  }

  Action<List<string>> getGraphsOnSelectedServerCallback;
  public void GetGraphsOnSelectedServer(Action<List<string>> callback)
  {
    getGraphsOnSelectedServerCallback = callback;
    StartCoroutine("GetGraphsOnSelectedServerHelper");
  }

  private IEnumerator GetGraphsOnSelectedServerHelper()
  {
    string query = $@"
      SELECT DISTINCT ?graph
      WHERE {{ 
        GRAPH ?graph {{ ?s ?p ?o }}
      }}";
    System.Net.HttpWebResponse results = endPoint.QueryRaw(query);
    yield return new WaitForEndOfFrame();

    StreamReader reader = new StreamReader(results.GetResponseStream());

    string nextLine = reader.ReadLine();
    string sparqlResult = nextLine;
    while (nextLine != null)
    {
      nextLine = reader.ReadLine();
      sparqlResult += nextLine;
      yield return new WaitForEndOfFrame();
    }

    XmlReader xmlReader = XmlReader.Create(new StringReader(sparqlResult));
    XElement xml = XElement.Load(xmlReader);
    XmlNameTable nameTable = new NameTable();
    XmlNamespaceManager namespaceManager = new XmlNamespaceManager(nameTable);
    string nameSpace = xml.GetDefaultNamespace().NamespaceName;
    namespaceManager.AddNamespace("ns", nameSpace);
    IEnumerable list = (IEnumerable)xml.XPathEvaluate("//ns:uri", namespaceManager);

    List<string> graphNames = new List<string>();
    foreach (XElement graph in list)
    {
      graphNames.Add(graph.Value);
    }
    getGraphsOnSelectedServerCallback(graphNames);
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
        {LanguageFilterString("?label")}
      }}
      ORDER BY ?label ?p LIMIT 100";
  //  Debug.Log("GetOutgoingPredicats: "+ query);
    endPoint.QueryWithResultSet(query, sparqlResultsCallback, state: null);
  }

  public void GetIncomingPredicats(string objectValue, SparqlResultsCallback sparqlResultsCallback)
  {
    string query = $@"
      {PREFIXES}
      select distinct ?p (STR(COUNT(?s)) AS ?count) STR(?label) AS ?label 
      where {{ 
        ?s ?p {objectValue} . 
        OPTIONAL {{
          ?p rdfs:label ?label
        }}
        {LanguageFilterString("?label")}
      }} 
      ORDER BY ?label ?p LIMIT 100";
 //   Debug.Log("GetIncomingPredicats: " + query);
    endPoint.QueryWithResultSet(query, sparqlResultsCallback, state: null);
  }

  public void GetLabelForPredicate(string uri, SparqlResultsCallback callback)
  {
    string query = $@"
        {PREFIXES}
        SELECT str(?label) as ?label WHERE {{
            OPTIONAL {{
                <{uri}> rdfs:label ?label .
                {LanguageFilterString("?label")}
            }}
        }}
        LIMIT 1";
    endPoint.QueryWithResultSet(query, (SparqlResultSet resultSet, object state) =>
    {
      if (resultSet == null)
      {
        callback(null, state);
        return;
      }
      if (resultSet.Count > 0 && resultSet[0].HasValue("label"))
      {
        callback(resultSet, state);
      }
      else
      {
        callback(null, state);
      }
    }, state: null);
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
      SparqlQueryParser parser = new();
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

  public void QuerySimilarPatternsMultipleLayers(string triples, string triplesWithOptional, OrderedDictionary orderByList, List<string> groupByList, bool additiveMode, Action<SparqlResultSet, string, string, bool> callback)
  {
    // TODO: make sure 'orderByList' do still exist
    string order = GetOrderByString(orderByList);
    string group = GetGroupByString(groupByList);
    string query = $@"
      {PREFIXES}
      select distinct * where {{
        {triplesWithOptional}
      }} {group} {order} LIMIT {queryLimit}";
    endPoint.QueryWithResultSet(query, (SparqlResultSet results, object state) =>
    {
      callback(results, query, triples, additiveMode);
    }, null);
  }

  public void CountQuerySimilarPatternsMultipleLayers(Graph graph, string triplesWithOptional, List<string> groupByList, Action<int> callback, string optionalVariable="*")
  {
    string group = GetGroupByString(groupByList);
    string query = $@"
      {PREFIXES}
      select count( distinct {optionalVariable}) as ?count where {{
        {triplesWithOptional}
      }} {group}";
    endPoint.QueryWithResultSet(query, (SparqlResultSet results, object state) =>
    {
      UnityMainThreadDispatcher.Instance().Enqueue(() =>
      {
        int count = 0;
        foreach (SparqlResult result in results)
        {
          result.TryGetValue("count", out INode iNode);
          ILiteralNode countNode = iNode as ILiteralNode;
          count = int.Parse(countNode.Value.ToString());
        }
        callback(count);
      });
    }, null);
  }
  

  public void AutocompleteSearch(string searchTerm, SparqlResultsCallback callback, Node variableNode = null)
  {
    if (searchTerm.Length > 3)
    {
      string query = GetAutoCompleteQuery(searchTerm, variableNode);
      endPoint.QueryWithResultSet(query, callback, state: null);
    }
    else
    {
      callback(null, null);
    }
  }

  private string GetAutoCompleteQuery(string searchTerm, Node variableNode)
  {
    if (Settings.Instance.databaseSupportsBifContains)
    {
      return GetAutoCompleteBifQuery(searchTerm, variableNode);
    }
    else
    {
      return GetAutoCompleteNonBifQuery(searchTerm, variableNode);
    }
  }

  private static string LanguageFilterString(string variableName)
  {
    if (Main.instance.languageCode == "")
    {
      return "";
    }
    else
    {
      return $"FILTER(LANG({variableName}) = '' || LANGMATCHES(LANG({variableName}), '{Main.instance.languageCode}')).";
    }
  }
  private static string GetAutoCompleteNonBifQuery(string searchTerm, Node variableNode)
  {
    if (variableNode != null)
    {
      return $@"
              {PREFIXES}
              select distinct {variableNode.GetQueryLabel()} AS ?uri (SAMPLE(?name) AS ?name)
              where {{
                {variableNode.graph.GetTriplesString()}
                {variableNode.GetQueryLabel()} rdfs:label ?name.
                ?uri(^(<>| !<>) | rdfs:label | skos:altLabel) ?entity.
                BIND(STR(?entity) AS ?name).
                FILTER REGEX(?name, '{searchTerm}', 'i').
              }}
              LIMIT {searchResultsLimit}";
    }
    else
    {
      return $@"
              {PREFIXES}
              select distinct ?uri (SAMPLE(?name) AS ?name) 
              where {{
                 ?uri(^(<>| !<>) | rdfs:label | skos:altLabel) ?entity.
                 BIND(STR(?entity) AS ?name).
                 FILTER REGEX(?name, '{searchTerm}', 'i')
              }}
              LIMIT {searchResultsLimit}";
    }
  }

  private string GetAutoCompleteBifQuery(string searchTerm, Node variableNode)
  {
    if (variableNode != null)
    {
      return $@"
               {PREFIXES}
               select distinct {variableNode.GetQueryLabel()} AS ?uri (SAMPLE(?name) AS ?name) 
               where {{
                 {variableNode.graph.GetTriplesString()}
                 {variableNode.GetQueryLabel()} rdfs:label ?name.
                 ?name bif:contains ""'{AddStar(searchTerm)}'"".
               }}
               LIMIT {searchResultsLimit}";
    }
    else
    {
      return $@"
              {PREFIXES}
              select distinct ?uri (SAMPLE(?name) AS ?name) 
              where {{
                ?uri rdfs:label ?name.
                ?name bif:contains ""'{AddStar(searchTerm)}'"".
              }}
              LIMIT {searchResultsLimit}";
    }
  }

  private string AddStar(string searchTerms)
  {
    if (searchTerms.Split(' ').Last().Length > 3)
    {
      return searchTerms + '*';
    }
    else
    {
      return searchTerms;
    }
  }

  public Dictionary<string, List<string>> RefineNode(IGraph refinmentGraph, string uri)
  {
    string query = $@"
            select ?label ?image
            where{{
                optional{{
                  <{uri}> <http://graph2vr.org/label> ?label .
                }}
                optional{{
                  <{uri}> <http://graph2vr.org/image> ?image .
                }}
            }}";

    SparqlResultSet data = refinmentGraph.ExecuteQuery(query) as SparqlResultSet;
    List<string> labels = new();
    List<string> images = new();

    foreach (SparqlResult result in data)
    {
      string label = ExtractVariableFrom(result, "label");
      string image = ExtractVariableFrom(result, "image");

      if (label != null)
      {
        labels.Add(label);
      }
      if (image != null)
      {
        images.Add(image);
      }
    }

    return new Dictionary<string, List<string>>
    {
      { "labels", labels },
      { "images", images },
    };
  }

  private string ExtractVariableFrom(SparqlResult result, string variable)
  {
    if (result.HasValue(variable))
    {
      return result.Value(variable).ToString();
    }
    return null;
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
    return "";
  }

  private static string GetGroupByString(List<string> groupByList)
  {
    if (groupByList.Count > 0)
    {
      string result = "Group By ";
      foreach (string group in groupByList)
      {
        result += $"{group} ";
      }
      return result;
    }
    return "";
  }

  private SparqlRemoteEndpoint GetEndPoint()
  {
    return new SparqlRemoteEndpoint(new Uri(Settings.Instance.sparqlEndpoint), Settings.Instance.baseURI);
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
