using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public enum Type { Subject, Object }

    public string uri = "";
    public string label = "";
    public Type type = Type.Subject;

    public List<Node> connectedNodes = new List<Node>();
    public List<Edge> connectedEdges = new List<Edge>();

    // Vairalbes for the Force-directed algorithm
    public Vector3 displacement;

    public void SetValue(string value)
    {
        this.uri = value;
        UpdateDisplay();
    }

    public void SetLabel(string label)
    {
        this.label = label;
        UpdateDisplay();
    }

    public string GetURI(string value)
    {
        return this.uri;
    }

    public string GetLabel()
    {
        return this.label;
    }

    public System.Uri GetURI()
    {
        return VDS.RDF.UriFactory.Create(this.uri);
    }

    private void UpdateDisplay()
    {
        string text = label;
        if (label == "") text = uri;
        TMPro.TextMeshPro test = GetComponentInChildren<TMPro.TextMeshPro>(true);
        test.text = text;
    }

    void Update()
    {
        transform.rotation = Quaternion.LookRotation(Camera.main.transform.position - transform.position, Vector3.up);
    }

}

