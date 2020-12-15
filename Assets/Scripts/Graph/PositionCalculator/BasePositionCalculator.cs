using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePositionCalculator
{
    protected Graph graph;
    public BasePositionCalculator(Graph graph)
    {
        this.graph = graph;
    }

    public virtual void SetInitial()
    {
        
    }

    public virtual void SetTimeStep()
    {

    }
}
