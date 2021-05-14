using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NodeMenu : MonoBehaviour
{
    private CircleMenu cm = null;
    public void Start()
    {
        cm = GetComponent<CircleMenu>();
    }

    public void Populate(Object input)
    {
        Node node = input as Node;
        Dictionary<string, int> set = Graph.instance.GetOutgoingPredicats(node.GetURIAsString());

        // Get uri of selected node
        // Get list of predicats 
        // call GetOutgoingPredicats()

        Close();
        
        foreach (KeyValuePair<string, int> item in set) {
            Debug.Log(item.Key);
            cm.AddButton(item.Key, Color.gray, () => {
                cm.Close();
            }, item.Value);
        }
        cm.AddButton("Test: Add more buttons", Color.gray, () => {
            cm.AddButton("Added button", Color.black, () => { });
            cm.ReBuild(cm.type);
        });
        cm.AddButton("Convert to Variable", Color.blue / 2, () => { });
        cm.AddButton("Convert to Constant", Color.cyan / 2, () => { });
        cm.AddButton("Close", Color.red / 2, () => {});
        cm.ReBuild(cm.type);
    }

    public void Close()
    {
        if(cm!=null) cm.Close();
    }
}
