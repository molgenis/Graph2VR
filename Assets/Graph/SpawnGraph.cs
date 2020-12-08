using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VDS.RDF;
using VDS.RDF.Query;

public class SpawnGraph : MonoBehaviour
{
    public float pullEnergie = 10;
    public float pushEnergie = 10;
    public float repulseDistance = 1f;

    public int nodesToBuild = 50;
    public GameObject nodePrefab;
    public GameObject linkPrefab;
    public List<GameObject> nodes = new List<GameObject>();
    public Canvas menu;
    private string sparqlQueryString = "select distinct <http://dbpedia.org/resource/Biobank> as ?s ?p ?o where { <http://dbpedia.org/resource/Biobank> ?p ?o } LIMIT 100";
    public class Links
    {
        public LineRenderer line;
        public Transform a;
        public Transform b;
        public string URI;
    }
    public List<Links> links = new List<Links>();
    public string SPARQLEndpoint = "http://dbpedia.org/sparql"; //dbpedia
    public string BaseURI = "http://dbpedia.org";
//    public string SPARQLEndpoint = "http://localhost:8890/sparql"; //local sparql endpoint from e.g. Virtuoso
//    public string BaseURI = "http://www.maelstrom.org/ontology/2020/10/"; //simple selfmade Ontology based on the maelstrom data 
 void SendQuery(string query) {
        SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new System.Uri(SPARQLEndpoint), BaseURI);

        SparqlResultSet results = endpoint.QueryWithResultSet(query);
        foreach (SparqlResult result in results)
        {
            //Debug.Log(result.ToString());
        }
        addNodes(results);
        // Adds only random links - this needs to be replaced!
        addLinks();
    }
    
    void Start()
    {
		SendQuery(sparqlQueryString);
    }
    
    //Used to Build a new graph just from the current query
    public void Rebuild()
    {
        // destroy all stuff
        for (int i = 0; i < nodes.Count; i++) {
            Destroy(nodes[i]);
        }
        for (int i = 0; i < links.Count; i++) {
            Destroy(links[i].line.gameObject);
        }
        nodes.Clear();
        links.Clear();
        // rebuild
        SendQuery(sparqlQueryString);
    }
    void Update()
    {
        updateLinks();
    }

	public void setSparqlQuery(string newQuery)
    {
        sparqlQueryString = newQuery;
    }

    public string getSparqlQuery(string newQuery)
    {
        return sparqlQueryString;
    }    
    
    //Saves a SPARQL query for all predicates of one URI in the sparqlQueryString (maximum: 100)
    //One example for a valid URI is "http://dbpedia.org/resource/Biobank"
    public void PredicatesForURI(string URI)
    {
        SendQuery("select distinct ?p where {<" + URI + "> ?p ?o} LIMIT 100");
    }
    
     //Saves a SPARQL query for all predicates of one URI in the sparqlQueryString 
     //Usually only one lable is expected, just in case there are mor, the maximum is 20
    //One example for a valid URI is "http://dbpedia.org/resource/Biobank"
    public void LabelForURI(string URI)
    {
        SendQuery("select distinct ?label where {<" + URI + "> rdfs:label ?label} LIMIT 20");
    }   

    void updateLinks()
    {
        for (int i = 0; i < links.Count; i++) {
            Links l = links[i];
            l.line.SetPosition(0, l.a.transform.position);
            l.line.SetPosition(1, l.b.transform.position);
        }
    }

    //Each node should have a URI and we might also want to add an information wether it already has been visited (like a previously visited Haperlink)
    void addNodes(SparqlResultSet query)
    {
        int i = 0;
        foreach (SparqlResult result in query) {
            GameObject clone = Instantiate(nodePrefab);
            clone.transform.SetParent(gameObject.transform);
            //clone.transform.parent = gameObject.transform;
            clone.transform.localPosition = Random.insideUnitSphere;
            clone.GetComponent<movement>().sg = this;
            clone.GetComponent<movement>().index = 1+(i % 50);
            clone.GetComponent<NodeInteraction>().menu = menu;
            TMPro.TextMeshPro test = clone.GetComponentInChildren<TMPro.TextMeshPro>(true);
            test.text = result.ToString();
            nodes.Add(clone);
            i++;
            if(i > 100)
            {
                break;
            }
        }
    }

    void addLinks()
    {
        for (int i = 0; i < nodes.Count; i++) {
            GameObject clone = Instantiate(linkPrefab);
            clone.transform.parent = gameObject.transform;
            clone.transform.localPosition = Random.insideUnitSphere;

            Links l = new Links();
            l.line = clone.GetComponent<LineRenderer>();
            l.a = nodes[i].transform;
            //l.a = nodes[Random.Range(0, (int)Mathf.Round(nodes.Count - 1))].transform;
            l.b = nodes[Random.Range(0, (int)Mathf.Round(nodes.Count - 1))].transform;

            l.a.gameObject.GetComponent<movement>().links.Add(l);
            l.b.gameObject.GetComponent<movement>().links.Add(l);

            links.Add(l);
        }
    }
}
