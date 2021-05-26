using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseLayoutAlgorithm : MonoBehaviour
{
    protected Graph graph;
    public void Awake()
    {
        graph = GetComponent<Graph>();
    }

    public virtual void CalculateLayout()
    {

    }

    public virtual void Stop()
    {

    }
}
