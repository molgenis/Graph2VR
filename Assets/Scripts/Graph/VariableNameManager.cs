using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariableNameManager
{
    private IDictionary<string, string> uriToVariable = new Dictionary<string, string>();
    private int counter = 1;

    public string GetVariableName(string uri)
    {
        if (uriToVariable.ContainsKey(uri)) {
            return uriToVariable[uri];
        } else {
            string name = "?variable" + counter;
            uriToVariable.Add(uri, name);
            counter++;
            return name;
        }
    }
}
