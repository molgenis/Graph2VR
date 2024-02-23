﻿using Dweiss;
using UnityEngine;

public class Main : MonoBehaviour
{
  public string languageCode = "en";

  static public Main instance;
  public Graph mainGraph = null;
  public GameObject graphPrefab;
  public VRKeys.Keyboard keyboard;

  public Texture2D line;
  public Texture2D lineDashed;

  void Start()
  {
    Main.instance.languageCode = PlayerPrefs.GetString("LanguageCode", "");

    VDS.RDF.Options.UsePLinqEvaluation = false;

    // NOTE: CODE FOR DEMO PURPOSE
    if (GameObject.FindGameObjectWithTag("UseCustomDatabase") != null)
    {
      Settings.Instance.sparqlEndpoint = PlayerPrefs.GetString("CustomServer", "http://localhost:8890/sparql");
      Settings.Instance.baseURI = PlayerPrefs.GetString("CustomGraphDatabase", "");
      Settings.Instance.databaseSupportsBifContains = false;
      Settings.Instance.searchOnKeypress = true;
      Settings.Instance.initialSparqlQueryString = $@"
            PREFIX owl: <http://www.w3.org/2002/07/owl#>
            PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
            Construct {{
               ?subject rdfs:label ?label.
               ?subject rdfs:subClassOf <http://www.w3.org/2002/07/owl#Thing>.
            }}
            WHERE {{
               {{ ?subject a owl:Class. }}
               OPTIONAL {{ ?subject rdfs:label ?label }}
               FILTER NOT EXISTS {{
                  ?subject rdfs:subClassOf ?otherSub .
                  FILTER(?otherSub != ?subject)
               }}
               FILTER(?subject != <http://www.w3.org/2002/07/owl#Thing>)
               FILTER(?subject != <http://www.w3.org/2002/07/owl#Nothing>)
            }}
            ORDER BY ?subject";
      QueryService.Instance.SwitchEndpoint();
    }
    // NOTE: END CODE FOR DEMO PURPOSE

    mainGraph = CreateGraph();

    if (Settings.Instance.startWithSingleNode)
    {
      mainGraph.CreateNode(Settings.Instance.initialSparqlURI, Vector3.zero);
    }
    else
    {
      mainGraph.CreateGraphBySparqlQuery(Settings.Instance.initialSparqlQueryString);
    }
  }

  private void Awake()
  {
    instance = this;
  }

  public Graph CreateGraph()
  {
    GameObject clone = Instantiate(graphPrefab);
    clone.transform.position = new Vector3(0, 2, 0);
    return clone.GetComponent<Graph>();
  }

  public Graph FindClosestGraphOrCreateNewGraph(Vector3 position)
  {
    Graph graph = Utils.FindClosestGraph(position)?.GetComponent<Graph>();
    if (graph == null)
    {
      graph = Main.instance.CreateGraph();
    }
    return graph;
  }

  public void ClearWorkspace()
  {
    GameObject[] graphs = GameObject.FindGameObjectsWithTag("Graph");
    foreach (GameObject graphGameObject in graphs)
    {
      Graph graph = graphGameObject.GetComponent<Graph>();
      Destroy(graph.boundingSphere.gameObject);
      Destroy(graphGameObject);
    }
  }
}
