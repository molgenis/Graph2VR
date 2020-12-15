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

    private void UpdateDisplay()
    {
        string text = label;
        if (label == "") text = uri;
        TMPro.TextMeshPro test = GetComponentInChildren<TMPro.TextMeshPro>(true);
        test.text = text;
    }

    void Update()
    {
        // Todo: implement better way to keep text aligned to view than this hack.
        TMPro.TextMeshPro test = GetComponentInChildren<TMPro.TextMeshPro>(true);
        test.transform.rotation = Quaternion.identity;
    }

}
