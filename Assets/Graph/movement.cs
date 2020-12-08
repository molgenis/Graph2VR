using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour
{
    public SpawnGraph sg;
    public List<SpawnGraph.Links> links = new List<SpawnGraph.Links>();
    public int index = 1;

    void Start()
    {

    }

    void Update()
    {
        /*if (Time.frameCount % index == 0)
        {
            // add force towards links
            for (int i = 0; i < links.Count; i++)
            {
                SpawnGraph.Links l = links[i];
                Rigidbody a = l.a.transform.GetComponent<Rigidbody>();
                Rigidbody b = l.b.transform.GetComponent<Rigidbody>();

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
