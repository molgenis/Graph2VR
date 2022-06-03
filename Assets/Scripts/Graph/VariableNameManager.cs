using System.Collections.Generic;
using VDS.RDF;

public class VariableNameManager
{
  private IDictionary<string, string> uriToVariable = new Dictionary<string, string>();
  private int counter = 1;

  public string GetVariableName(INode node)
  {
    if (node == null)
    {
      string name = "?variable" + counter;
      counter++;
      return name;
    }
    else
    {
      string uri = node.ToString();
      if (node.NodeType == NodeType.Variable) return (node as VariableNode).VariableName;
      if (node.NodeType == NodeType.Uri && uriToVariable.ContainsKey(uri))
      {
        return uriToVariable[uri];
      }
      else
      {
        string name = "?variable" + counter;
        if (node.NodeType == NodeType.Uri) uriToVariable.Add(uri, name);
        counter++;
        return name;
      }
    }
  }

}
