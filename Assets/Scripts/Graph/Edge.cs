using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge : MonoBehaviour
{
    public string uri;
    public Node from;
    public Node to;
    private LineRenderer renderer;
    private void Start()
    {
        renderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        // Update line position
        renderer.SetPosition(0, from.transform.position);
        renderer.SetPosition(1, to.transform.position);
    }

}
