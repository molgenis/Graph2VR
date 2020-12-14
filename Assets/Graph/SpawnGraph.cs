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
    public int index = 0;
    public GameObject nodePrefab;
    public GameObject EdgePrefab;
    public List<Triple> ts = new List<Triple>();
    public List<GameObject> nodes = new List<GameObject>();
    public Canvas menu;
    private string sparqlQueryString = "select distinct <http://dbpedia.org/resource/Biobank> as ?s ?p ?o where { <http://dbpedia.org/resource/Biobank> ?p ?o } LIMIT 100";
    public class edge
    {
        public LineRenderer line;
        public Transform a;
        public Transform b;
        public string URI;
    }

    public class Triple
    {
        public string Subject;
        public string Predicate;
        public string Object;
    }

    public List<edge> edges = new List<edge>();
    public string SPARQLEndpoint = "http://dbpedia.org/sparql"; //dbpedia
    public string BaseURI = "http://dbpedia.org";
//    public string SPARQLEndpoint = "http://localhost:8890/sparql"; //local sparql endpoint from e.g. Virtuoso
//    public string BaseURI = "http://www.maelstrom.org/ontology/2020/10/"; //simple selfmade Ontology based on the maelstrom data 
 void SendQuery(string query) {
        SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new System.Uri(SPARQLEndpoint), BaseURI);
        Triple triple = new Triple();
        SparqlResultSet results = endpoint.QueryWithResultSet(query);
        foreach (SparqlResult result in results)
        {
            INode s = null;
            INode p = null;
            INode o = null;
            triple = new Triple();
            result.TryGetValue("s", out s);
            triple.Subject = s.ToString();
            addNode(s);
            result.TryGetValue("p", out p);
            triple.Predicate = s.ToString();
            result.TryGetValue("o", out o);
            addNode(o);
            triple.Object = o.ToString();
            ts.Add(triple);
            //Debug.Log(result.ToString());
        }
        //addNodes(results);
        // Adds only random links - this needs to be replaced!
        addEdges();
    }

    void addNode(INode node) {
        if (GameObject.Find(node.ToString()) == null)
        {
            GameObject clone = Instantiate(nodePrefab);
            clone.transform.SetParent(gameObject.transform);
            clone.transform.localPosition = Random.insideUnitSphere;
            clone.GetComponent<movement>().sg = this;
            clone.GetComponent<movement>().index = 1 + (index % 50);
            index += 1;
            clone.GetComponent<NodeInteraction>().menu = menu;
            clone.name = node.ToString();
            TMPro.TextMeshPro test = clone.GetComponentInChildren<TMPro.TextMeshPro>(true);
            test.text = node.ToString();
            nodes.Add(clone);
        }
        else {
            //node is already there
            return;
        }
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
        for (int i = 0; i < edges.Count; i++) {
            Destroy(edges[i].line.gameObject);
        }
        nodes.Clear();
        edges.Clear();
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
        for (int i = 0; i < edges.Count; i++) {
            edge e = edges[i];
            e.line.SetPosition(0, e.a.transform.position);
            e.line.SetPosition(1, e.b.transform.position);
        }
    }

    //Each node should have a URI and we might also want to add an information wether it already has been visited (like a previously visited Haperlink)
   /* void addNodes(SparqlResultSet query)
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
            clone.name = "uri";
            TMPro.TextMeshPro test = clone.GetComponentInChildren<TMPro.TextMeshPro>(true);
            test.text = result.ToString();
            nodes.Add(clone);
            i++;
            if(i > 100)
            {
                break;
            }
        }
    }*/

    void addEdges()
    {
        for (int i = 0; i < ts.Count; i++) {
            GameObject clone = Instantiate(EdgePrefab);
            clone.transform.parent = gameObject.transform;
            clone.transform.localPosition = Random.insideUnitSphere;

            edge e = new edge();
            e.line = clone.GetComponent<LineRenderer>();
            //we need to get the node.transform where the URI == triple.Subject
            if (GameObject.Find(ts[i].Subject) != null)
            {
                e.a = GameObject.Find(ts[i].Subject).transform;
                e.a.gameObject.GetComponent<movement>().edges.Add(e);
            }
            else {
                //break;
            }
            if (GameObject.Find(ts[i].Object) != null)
            {
                e.b = GameObject.Find(ts[i].Object).transform;
                e.b.gameObject.GetComponent<movement>().edges.Add(e);
            }
            else
            {
                //break;
            }
            edges.Add(e);
        }
    }
}
