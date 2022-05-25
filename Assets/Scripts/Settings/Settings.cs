/*******************************************************
 * Copyright (C) 2017 Doron Weiss  - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of unity license.
 * 
 * See https://abnormalcreativity.wixsite.com/home for more info
 *******************************************************/

using UnityEngine;

namespace Dweiss
{
  [System.Serializable]
  public class Settings : ASettings
  {  // Temporary removed baseclase ASettings
    [Header("--Main settings--")]
    public string SparqlEndpoint = "https://dbpedia.org/sparql";
    //public string BaseURI = "";
    //"https://github.com/PjotrSvetachov/GraphVR/example-graph";
    // "http://dbpedia.org";

    public string BaseURI = "http://dbpedia.org"; //"https://github.com/PjotrSvetachov/GraphVR/example-graph";
    public string DefaultNodeCreationURI = "http://graph2vr.org/newNode#";
    public string DefaultEdgeCreationURI = "http://graph2vr.org/newEdge#";

    // We can start with an single node or a query.
    public string initialSparqlQueryString = "construct {?s ?p ?o}  where  {?s ?p ?o} Limit 100";

    public string initialSparqlURI = "";

    public bool StartWithSingleNode = false;

    public string[] ImagePredicates = { "http://xmlns.com/foaf/0.1/depiction", "http://xmlns.com/foaf/0.1/Image", "http://xmlns.com/foaf/0.1/thumbnail", "http://dbpedia.org/property/photo" };

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
