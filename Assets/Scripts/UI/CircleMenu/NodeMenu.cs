using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NodeMenu : MonoBehaviour
{
    private CircleMenu cm = null;
    public bool predicatMode = true;
    private Dictionary<string, int> set;
    private Node node = null;

    public void Start()
    {
        cm = GetComponent<CircleMenu>();
    }

    public void GetPredicats()
    {
        if (node != null) {
            if (predicatMode) {
                set = Graph.instance.GetOutgoingPredicats(node.GetURIAsString());
            } else {
                set = Graph.instance.GetIncomingPredicats(node.GetURIAsString());
            }
        }
    }

    public void Populate(Object input)
    {
        node = input as Node;
        GetPredicats();

        if (set != null) {
            // Get uri of selected node
            // Get list of predicats 
            // call GetOutgoingPredicats()

            Close();

            if (predicatMode) {
                cm.AddButton("List incoming predicats", Color.blue / 2, () => {
                    predicatMode = false;
                    Populate(input); 
                });
            } else {
                cm.AddButton("List outgoing predicats", Color.blue / 2, () => {
                    predicatMode = true;
                    Populate(input);
                });
            }

            foreach (KeyValuePair<string, int> item in set) {
                cm.AddButton(item.Key, Color.gray, () => {
                    Graph.instance.ExpandGraph(node, item.Key, predicatMode);
                    cm.Close();
                }, item.Value);
            }
            cm.AddButton("Convert to Variable", Color.blue / 2, () => { });
            cm.AddButton("Convert to Constant", Color.cyan / 2, () => { });
            cm.AddButton("Close", Color.red / 2, () => { });
            cm.ReBuild(cm.type);
        }
    }

    public void Close()
    {
        if(cm!=null) cm.Close();
    }
}
