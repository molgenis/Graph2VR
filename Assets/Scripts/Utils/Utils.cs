using System;
using System.Collections.Generic;
using VDS.RDF.Query;
using System.Linq;
using VDS.RDF;

public class Utils
{
  static public string GetShortLabelFromUri(string uri)
  {
    var list = uri.Split('/', '#');
    if (list.Length > 0) {
      return list[list.Length - 1];
    }
    else
    {
      return uri;
    }
  }

  static public Dictionary<string, Tuple<string, int>> GetPredicatsList(SparqlResultSet sparqlResults)
  {
    return sparqlResults.Aggregate(new Dictionary<string, Tuple<string, int>>(), (accum, result) =>
    {
      result.TryGetValue("p", out INode predicate);
      result.TryGetValue("count", out INode countNode);
      result.TryGetValue("label", out INode labelNode);

      string label = labelNode != null ? labelNode.ToString() : "";

      if (predicate != null)
      {
        string predicateString = predicate.ToString();
        int count = int.Parse(countNode.ToString());
        Tuple<string, int> value = new Tuple<string, int>(label, count);
        if (!accum.ContainsKey(predicateString))
        {
          accum.Add(predicateString, value);
        }
        else
        {
          // Why overwrite and not just skip?
          accum[predicateString] = value;
        }
      }
      return accum;
    });
  }
}

