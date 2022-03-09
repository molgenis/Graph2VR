using UnityEngine;

public class Main : MonoBehaviour
{
  public string languageCode = "en";
  public string initialSparqlQueryString = "select distinct <http://dbpedia.org/resource/Biobank> as ?s ?p ?o where { <http://dbpedia.org/resource/Biobank> ?p ?o } LIMIT 100";

  static public Main instance;
  public Graph mainGraph = null;
  public GameObject graphPrefab;

  void Start()
  {
    mainGraph = CreateGraph();
    mainGraph.CreateGraphBySparqlQuery(initialSparqlQueryString);
  }

  private void Awake()
  {
    instance = this;
  }

  public Graph CreateGraph()
  {
    GameObject clone = Instantiate(graphPrefab);
    clone.transform.position = new Vector3(0, 1, 0);
    return clone.GetComponent<Graph>();
  }
}
