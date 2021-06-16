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
        Refine();
    }
    
    public void Refine()
    {
        foreach (Triple t in iNode.Graph.GetTriplesWithSubject(iNode)) {
            // rdfs:subClassOf -> relations
            // owl:equivalentClass
            // owl:Class, owl:ObjectProperty, owl:disjointWith
            // rdfs:Resource, rdf:Property
            // owl:DeprecatedClass, owl:DeprecatedProperty
            // rdfs:Datatype, rdfs:Literal
            // owl:DatatypeProperty
            // owl:disjointWith, owl:unionOf, owl:intersectionOf, owl:ComplementOf

            // rdfs:Label -- shown as text
            // <url> / rdfs:Type / "image" -- show
            // foaf: Image -- renderd on circle
            // rdfs:Literal -- change color
            // owl:Class, owl:ObjectProperty -- change to lightblue color

            // http://vowl.visualdataweb.org/v2/#rdfsResource
            // Filter out all common metadata, put in in this node (like labels, classes, image tags ) 
            // (display detail on selection?)
            // Remove those connecting nodes from graph (hide)

            if (t.Predicate.ToString() == "http://www.w3.org/2000/01/rdf-schema#label") {
                SetLabel(t.Object.ToString());
                Graph.instance.Hide(t.Object);
            }

        }
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
        this.label = label.Replace("@" + Main.instance.languageCode, "");
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
        // Depricated
        string query = "prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>  select STR(?label) AS ?label where { <" + uri + "> rdfs:label ?label . FILTER(LANG(?label) = '' || LANGMATCHES(LANG(?label), '" + Main.instance.languageCode + "')) } LIMIT 1";

        endpoint.QueryWithResultSet(query, (results, state) => {
            results[0].TryGetValue("label",  out INode label);
            SetLabel(label.ToString());
        }, (object)this);
    }
/*
    public bool RequestIsClass(SparqlRemoteEndpoint endpoint)
    {
        string query = "prefix rdfs: http://www.w3.org/2000/01/rdf-schema#  ASK {{<" + uri + "> a owl:Class.} UNION {?anything a <" + uri + ">.}}";
        try {
            SparqlResultSet result = endpoint.QueryWithResultSet(query);
            Debug.Log(result.Result.ToString());
            return result.Result;
        } catch { Debug.Log(uri); }
        return false;

    }
    */
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

