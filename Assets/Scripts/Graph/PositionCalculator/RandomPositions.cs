using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPositions : BasePositionCalculator
{
    public RandomPositions(Graph graph) : base(graph)
    {
    }

    public override void SetInitial()
    {
        foreach(Node node in graph.nodeList) {
            node.transform.position = Random.insideUnitSphere * 3f;
        }
    }

    public override void SetTimeStep()
    {

    }
}
