using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruchtermanReingold : BaseLayoutAlgorithm
{
    // variables for the Fruchterman-Reingold algorithm
    public float Temperature = 0;

    public override void CalculateLayout()
    {
        Temperature = 0.05f;
    }

    public override void Stop()
    {
        Temperature = 0f;
    }

    void Update()
    {
        if (Temperature > 0.01f) {
            FruchtermanReingoldIteration();
        }
    }

    public float C_CONSTANT = 1.0f;
    public float AREA_CONSTANT = 1.0f;
    //Function for the Fruchterman-Reingold algorithm
    private float Fa(float x)
    {
        float k = (C_CONSTANT * Mathf.Sqrt(AREA_CONSTANT / graph.nodeList.Count));
        return 0.001f * (x * x * x * x) / k;
    }

    //Function for the Fruchterman-Reingold algorithm
    private float Fr(float x)
    {
        float k = (C_CONSTANT * Mathf.Sqrt(AREA_CONSTANT / graph.nodeList.Count));
        return 0.001f * (k * k) / (x * x * x);
    }

    // Do one iteration fo the Fruchterman-Reingold algorithm
    // We only use localpositions so the algorithm stays stable when zooming in/out
    public void FruchtermanReingoldIteration()
    {
        // calculate repulsive forces
        foreach (Node node in graph.nodeList) {
            node.displacement = Vector3.zero;
            foreach (Node neightbor in graph.nodeList) {
                if (node != neightbor) {
                    Vector3 delta = node.transform.localPosition - neightbor.transform.localPosition;
                    float FrForce = Fr(delta.magnitude);
                    node.displacement += delta.normalized * FrForce;
                }
            }
        }

        // calculate attractive forces
        foreach (Edge edge in graph.edgeList) {
            if (edge.displayObject != null && edge.displaySubject != null) {
                Vector3 delta = edge.displayObject.transform.localPosition - edge.displaySubject.transform.localPosition;
                float FaForce = Fa(delta.magnitude);
                Vector3 normal = delta.normalized;
                edge.displayObject.displacement -= normal * FaForce;
                edge.displaySubject.displacement += normal * FaForce;
            }
        }

        // Reposition the nodes, taking ionto account the temperature
        float TotalDisplacement = 0.0f;
        foreach (Node node in graph.nodeList) {
            if (node != null) {
                float DisplacementMagitude = node.displacement.magnitude;
                TotalDisplacement = Mathf.Max(DisplacementMagitude, TotalDisplacement);
                node.transform.localPosition += (node.displacement / DisplacementMagitude) * Mathf.Min(DisplacementMagitude, Temperature);
            }
        }

        // reduce the temperature
        Temperature -= 0.005f * Time.deltaTime;
    }
}
