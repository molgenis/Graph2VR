/*******************************************************
 * Copyright (C) 2017 Doron Weiss  - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of unity license.
 * 
 * See https://abnormalcreativity.wixsite.com/home for more info
 *******************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace Dweiss
{
  [System.Serializable]
  public class DatabaseSetttings
  {
    public string label = "";
    public string sparqlEndpoint = "https://dbpedia.org/sparql";
    public string baseURI = "http://dbpedia.org"; //"https://github.com/PjotrSvetachov/GraphVR/example-graph";
    public bool databaseSuportsBifContains = false;
    public bool searchOnKeypress = false;
  }

  [System.Serializable]
  public class Settings : ASettings
  {  // Temporary removed baseclase ASettings
    [Header("--Main settings--")]
    //public string BaseURI = "";
    //"https://github.com/PjotrSvetachov/GraphVR/example-graph";
    // "http://dbpedia.org";

    public string sparqlEndpoint = "https://dbpedia.org/sparql";
    public string baseURI = "http://dbpedia.org"; //"https://github.com/PjotrSvetachov/GraphVR/example-graph";
    public bool databaseSuportsBifContains = false;

    public string defaultNodeCreationURI = "http://graph2vr.org/newNode#";
    public string defaultEdgeCreationURI = "http://graph2vr.org/newEdge#";

    // We can start with an single node or a query.
    public string initialSparqlQueryString = "construct {?s ?p ?o}  where  {?s ?p ?o} Limit 100";
    // prefix dbo: <http://dbpedia.org/ontology/> prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> prefix dbp: <http://dbpedia.org/property/> prefix dbr: <http://dbpedia.org/resource/> construct {     ?mountain rdf:type dbo:Mountain.     ?mountain dbo:elevation ?height.     ?mountain dbp:location ?location }  where {     ?mountain rdf:type dbo:Mountain.     ?mountain dbo:elevation ?height.     ?mountain dbp:location ?location }  ORDER BY DESC (?height)  LIMIT 10
    /*
     prefix dbo: <http://dbpedia.org/ontology/> 
      prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
      prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
      prefix dbp: <http://dbpedia.org/property/> 
      prefix dbr: <http://dbpedia.org/resource/> 
      construct { ?mountain rdf:type dbo:Mountain.     
                  ?mountain dbo:elevation ?height.     
                  ?mountain dbp:location ?location.
                  ?mountain <http://graph2vr.org/label> ?label.
                  ?mountain <http://graph2vr.org/image> ?image.
                 }  
      where {     ?mountain rdf:type dbo:Mountain.     
                  ?mountain dbo:elevation ?height.     
                  ?mountain dbp:location ?location. 
                  Optional{?mountain rdfs:label ?label.}
                  Optional{?mountain dbp:depiction ?image.}
      }  
      ORDER BY DESC (?height)  LIMIT 10
    */
    public string initialSparqlURI = "";
    public bool startWithSingleNode = false;
    public bool searchOnKeypress = false;
    public float playerHeight = 1.8f;

    public string[] imagePredicates = { "http://xmlns.com/foaf/0.1/depiction", "http://xmlns.com/foaf/0.1/Image", "http://xmlns.com/foaf/0.1/thumbnail", "http://dbpedia.org/property/photo" };

    public string[] infopanelPredicates = { "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://www.w3.org/2000/01/rdf-schema#label" };

    // VOWL color schema
    // Datatype 	#fc3 	yellow 	rdfs:Datatype, rdfs:Literal
    public Color literalColor = new Color(255, 204, 51);
    // General 	#acf 	light blue 	owl:Class, owl:ObjectProperty (incl. subclasses)
    public Color nodeOwlClassColor = new Color(170, 204, 255);
    // Rdf 	#c9c 	light purple 	rdfs:Class, rdfs:Resource, rdf:Property
    public Color nodeRdfsClassColor = new Color(204, 153, 204);
    // Datatype Property 	#9c6 	light green owl:DatatypeProperty
    public Color nodeOwlDatatypeColor = new Color(153, 204, 102);
    // Deprecated 	#ccc 	light gray 	owl:DeprecatedClass, owl:DeprecatedProperty
    public Color deprecatedColor = new Color(204, 204, 204);
    // Neutral 	#fff 	white   owl:Thing, arrowhead of rdfs:subClassOf
    public Color arrowheadSubclassOfColor = new Color(255, 255, 255);

    public List<DatabaseSetttings> databaseSetttings = new List<DatabaseSetttings>();

    private void Awake()
    {
      // base.Awake ();
      SetupSingelton();
    }

    new string name = "";
    #region  Singleton
    public static Settings _instance;

    public static Settings Instance { get { return _instance; } }
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
}
