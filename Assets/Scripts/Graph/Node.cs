using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VDS.RDF;
using VDS.RDF.Query;

public class Node : MonoBehaviour
{
    public string uri = ""; // Full URI, empty if literal
    public string label = "";

    public INode iNode;
    public Color defaultColor;
    public List<Node> connectedNodes = new List<Node>();
    public List<Edge> connectedEdges = new List<Edge>();

    // Vairalbes for the Force-directed algorithm
    public Vector3 displacement;
    public void Start()
    {
        InvokeRepeating("SlowUpdate", 1, 1);
    }

    public void SetDefaultColor(Color color)
    {
        defaultColor = color;
        GetComponent<Renderer>().material.color = color;
    }

    public void SetColor(Color color)
    {
        GetComponent<Renderer>().material.color = color;
    }

    public void SetLabel(string label)
    {
        this.label = label;
        UpdateDisplay();
    }

    public string GetLabel()
    {
        return this.label;
    }

    public string GetURIAsString()
    {
        return this.uri;
    }

    public void SetURI(string uri)
    {
        this.uri = uri;
    }

    public System.Uri GetURI()
    {
        return VDS.RDF.UriFactory.Create(this.uri);
    }

    public void RequestLabel(SparqlRemoteEndpoint endpoint)
    {
        string query = "prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>  select STR(?label) AS ?label where { <" + uri + "> rdfs:label ?label . FILTER(LANG(?label) = '' || LANGMATCHES(LANG(?label), '" + Main.instance.languageCode + "')) } LIMIT 1";

        endpoint.QueryWithResultSet(query, (results, state) => {
            results[0].TryGetValue("label",  out INode label);
            SetLabel(label.ToString());
        }, (object)this);
    }

    private void UpdateDisplay()
    {
        string text = label;
        if (label == "") text = uri;
        TMPro.TextMeshPro test = GetComponentInChildren<TMPro.TextMeshPro>(true);
        test.text = text;
    }

    void SlowUpdate()
    {
        UpdateDisplay();
    }

    void Update()
    {
        transform.rotation = Quaternion.LookRotation(Camera.main.transform.position - transform.position, Vector3.up);
    }

}

