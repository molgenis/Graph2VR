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
    public class Links
    {
        public LineRenderer line;
        public Transform a;
        public Transform b;
    }
    public List<Links> links = new List<Links>();

    void Start()
    {
        SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new System.Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");

        SparqlResultSet results = endpoint.QueryWithResultSet("SELECT DISTINCT ?Concept WHERE {[] a ?Concept}");
        foreach (SparqlResult result in results)
        {
            //Debug.Log(result.ToString());
        }
        addNodes(results);
        addLinks();
    }
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
        SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new System.Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");

        SparqlResultSet results = endpoint.QueryWithResultSet("SELECT DISTINCT ?Concept WHERE {[] a ?Concept}");
        foreach (SparqlResult result in results)
        {
            //Debug.Log(result.ToString());
        }
        addNodes(results);
        addLinks();
    }
    void Update()
    {
        updateLinks();


    }

    void updateLinks()
    {
        for (int i = 0; i < links.Count; i++) {
            Links l = links[i];
            l.line.SetPosition(0, l.a.transform.position);
            l.line.SetPosition(1, l.b.transform.position);
        }
    }

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
