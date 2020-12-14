using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour
{
    public SpawnGraph sg;
    public List<SpawnGraph.edge> edges = new List<SpawnGraph.edge>();
    public int index = 1;

    void Start()
    {

    }

    // when we realize, that this is too inefficient when we have too many nodes, we might optimize this by using a 3d Barnes-Hut Algorithm
    //Explanation: http://arborjs.org/docs/barnes-hut
    //Example implementation (2d) in Unity: https://forum.unity.com/threads/barnes-hut.292885/

    void Update()
    {
        /*if (Time.frameCount % index == 0)
        {
            // add force towards links
            for (int i = 0; i < edges.Count; i++)
            {
                SpawnGraph.edge e = edges[i];
                Rigidbody a = e.a.transform.GetComponent<Rigidbody>();
                Rigidbody b = e.b.transform.GetComponent<Rigidbody>();

                Vector3 normal = b.transform.localPosition - a.transform.localPosition;
                normal.Normalize();
                a.AddForce(normal * sg.pullEnergie * Time.deltaTime);
                b.AddForce(-normal * sg.pullEnergie * Time.deltaTime);
            }

            Rigidbody me = gameObject.GetComponent<Rigidbody>();
            float tmp = gameObject.transform.localToWorldMatrix.MultiplyPoint(new Vector3(1, 0, 0)).magnitude;
            me.mass = tmp;
            // repulse all closeby nodes

            for (int i = 0; i < sg.nodes.Count; i++)
            {
                Rigidbody target = sg.nodes[i].GetComponent<Rigidbody>();
                if (target == me) continue;
                float distance = Vector3.Distance(target.transform.localPosition, transform.localPosition);
                if (distance < sg.repulseDistance)
                {
                    // lets repulse this one!
                    Vector3 normal = target.transform.localPosition - me.transform.localPosition;
                    normal.Normalize();
                    float power = (1 - (distance / sg.repulseDistance)) * sg.pushEnergie * Time.deltaTime;
                    target.AddForce(normal * power);
                    me.AddForce(-normal * power);
                }
            }
        }*/
    }
}
