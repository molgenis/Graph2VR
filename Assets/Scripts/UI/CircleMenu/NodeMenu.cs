using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeMenu : MonoBehaviour
{
    private CircleMenu cm = null;
    public void Start()
    {
        cm = GetComponent<CircleMenu>();
    }

    public void Populate()
    {
        Close();
        List<string> subjects = Graph.instance.GetSubjects();
        foreach (string subject in subjects) {
            cm.AddButton(subject, Color.gray, () => {
                cm.Close();
            }, Random.Range(1, 10));
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
