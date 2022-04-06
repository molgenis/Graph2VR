using UnityEngine;
using Dweiss;

public class Main : MonoBehaviour
{
  public string languageCode = "en";

  static public Main instance;
  public Graph mainGraph = null;
  public GameObject graphPrefab;

  void Start()
  {
    mainGraph = CreateGraph();
    if (Settings.Instance.StartWithSingleNode)
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
    clone.transform.position = new Vector3(0, 1, 0);
    return clone.GetComponent<Graph>();
  }
}
