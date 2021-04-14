using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleMenuTest : MonoBehaviour
{
    // TODO: remove me, a am just here for development testing
    void Start()
    {
        CircleMenu cm = GetComponent<CircleMenu>();
        cm.AddButton("Hello World", Color.gray, () => { }, 10);
        cm.AddButton("What happens if we put a lot of text here, like a lot, more than this. something like this maybe.", Color.gray, () => { });
        cm.AddButton("Hello World", Color.gray, () => { });
        cm.AddButton("Hello World", Color.gray, () => { }, 5);
        cm.AddButton("Hello World", Color.gray, () => { }, 3);
        cm.AddButton("Hello World", Color.gray, () => { }, 1);
        cm.AddButton("Add more buttons", Color.gray, () => {
            cm.AddButton("Added button", Color.gray, () => { });
            cm.ReBuild(cm.type);
        });
        cm.AddButton("Convert to Variable", Color.blue / 2, () => { }, 3);
        cm.AddButton("Convert to Constant", Color.cyan / 2, () => { });
        cm.AddButton("Close", Color.red / 2, () => { }, 0);

        cm.ReBuild(cm.type);
    }
}
