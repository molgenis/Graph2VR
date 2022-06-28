using Dweiss;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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

   public void ExecuteQuery(string query, GraphCallback queryCallback)
   {
      try
      {
         endPoint.QueryWithResultGraph(query, queryCallback, state: null);
      }
      catch (RdfQueryException error)
      {
         Debug.Log("No database connection found");
         Debug.Log(error);
      }
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
      string nodeUriString = node.GetURIAsString();
      if (isOutgoingLink)
      {

         // Select with label
         return $@"
            {PREFIXES}
            construct {{
                <{nodeUriString}> <{uri}> ?object .
            }} where {{
                <{nodeUriString}> <{uri}> ?object .
            }} 
            LIMIT {queryLimit}";
      }
      else
      {
         return $@"
            {PREFIXES}
            construct {{
                ?subject <{uri}> <{nodeUriString}> .
            }} where {{
                ?subject <{uri}> <{nodeUriString}>
            }}  
            LIMIT {queryLimit}";
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
            ?object ?graph2vrlabel ?label .
            ?object ?graph2vrimage ?image .
            ?object a ?type .
        }} where {{
            <{nodeUriString}> <{uri}> ?object .
            {GetOptionalGraphQuery("?object")}
        }} 
        LIMIT {queryLimit}";
      }
      else
      {
         return $@"
        {PREFIXES}
        construct {{
            ?subject <{uri}> <{nodeUriString}> .
            ?subject ?graph2vrlabel ?label .
            ?subject ?graph2vrimage ?image .
            ?subject a ?type .
        }} where {{
            ?subject <{uri}> <{nodeUriString}>
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
          Select {variable} <http://graph2vr.org/label> AS ?graph2vrlabel STR(?label) as ?label
          where {{
            {variable} rdfs:label ?label .
            FILTER(LANG(?label) = '' || LANGMATCHES(LANG(?label), '{Main.instance.languageCode}'))
          }}
        }}

        Optional{{
          Select {variable} <http://graph2vr.org/image> AS ?graph2vrimage ?image
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
      Debug.Log("QuerySimilarPatternsMultipleLayers query service");
      // TODO: make sure 'orderByList' do still exist
      string order = GetOrderByString(orderByList);
      string query = $@"
      {PREFIXES}
      select distinct * where {{
        {triples}
      }} {order} LIMIT {queryLimit}";

      endPoint.QueryWithResultSet(query, (SparqlResultSet results, object state) =>
      {
         Debug.Log("QuerySimilarPatternsMultipleLayers query service internal callback");
         callback(results, query);
      }, null);
   }


   public void AutocompleteSearch(string searchterm, SparqlResultsCallback callback)
   {
      if (searchterm.Length > 3)
      {
         string query = $@"
      {PREFIXES}
      select distinct ?uri ?name 
      where {{
      ?uri(^(<>| !<>) | rdfs:label | skos:altLabel) ?entity.
      BIND(STR(?entity) AS ?name).
      FILTER REGEX(?name, '{searchterm}')
      }}
      LIMIT 5";

         string bifQuery = $@"
      {PREFIXES}
      select distinct ?uri ?name 
      where {{
        ?uri rdfs:label ?name.
        ?name bif:contains '{searchterm}'.
      }}
      LIMIT 5";

         endPoint.QueryWithResultSet(
           Settings.Instance.databaseSuportsBifContains ? bifQuery : query
           , callback, state: null);
      }
      else
      {
         callback(null, null);
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
      List<string> labels = new List<string>();
      List<string> images = new List<string>();

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
      INode selected = null;
      if (result.HasValue(variable))
      {
         selected = result.Value(variable);
         return selected.ToString();
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
      else
      {
         return "";
      }
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
