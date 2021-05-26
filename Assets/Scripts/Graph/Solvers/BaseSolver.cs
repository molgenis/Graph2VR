using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSolver : MonoBehaviour
{
    protected Graph graph;
    public void Awake()
    {
        graph = GetComponent<Graph>();
    }

    public virtual void Solve()
    {

    }

    public virtual void Stop()
    {

    }
}
