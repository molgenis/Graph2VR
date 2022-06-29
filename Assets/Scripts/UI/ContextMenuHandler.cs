using Dweiss;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VDS.RDF;

public class ContextMenuHandler : MonoBehaviour
{
  public GameObject ContentPanel;
  public GameObject labelPrefab;
  InputField inputPrefab;

  // Start is called before the first frame update
  List<GameObject> labels = new List<GameObject>();
  public delegate void OnItemIsSelected(string button);

  private List<Triple> getTripplesWithPredicate(IGraph graph, string predicate)
  {
    List<Triple> result = new List<Triple>();
    foreach (Triple triple in graph.Triples)
    {
      if (triple.Predicate.ToString() == predicate)
      {
        result.Add(triple);
      }
    }
    return result;
  }

  public void Initiate(Node node)
  {
    GameObject loadingLabel = AddLabel("Loading please wait...", 20);
    GetComponentInChildren<Button>().onClick.AddListener(() => node.ToggleInfoPanel());
    labelPrefab = Resources.Load<GameObject>("UI/Label");
    QueryService.Instance.GetDescriptionAsync(node.uri, (graph, state) =>
    {
      UnityMainThreadDispatcher.Instance().Enqueue(() =>
      {
        Destroy(loadingLabel.gameObject);
        HashSet<INode> predicates = new HashSet<INode>();
        foreach (Triple triple in graph.Triples)
        {
          predicates.Add(triple.Predicate);
        }
        int numAdded = 0;
        AddLabel(node.uri, 20);
        foreach (string predicate in Settings.Instance.infopanelPredicates)
        {
          string prediacteName = node.graph.GetShortName(predicate);
          prediacteName = prediacteName != "" ? prediacteName : predicate;
          string property = "";
          string isOf = "";
          IEnumerable<Triple> triples = getTripplesWithPredicate(graph, predicate);
          bool hasPropery = false;
          bool isPropery = false;
          foreach (Triple triple in triples)
          {
            string subject = triple.Subject.ToString();
            string obj = triple.Object.ToString();
            if (subject.Equals(node.uri))
            {
              string shortName = node.graph.GetShortName(obj);
              if (shortName != "")
              {
                property += shortName + "\n";
              }
              else
              {
                property += obj + "\n";
              }
              hasPropery = true;
              numAdded++;
            }
            else if (obj.Equals(node.uri))
            {
              string shortName = node.graph.GetShortName(subject);
              if (shortName != "")
              {
                isOf += shortName + "\n";
              }
              else
              {
                isOf += subject + "\n";
              }
              isPropery = true;
              numAdded++;
            }
            if (numAdded > 100)
            {
              break;
            }
          }
          if (hasPropery)
          {
            AddLabel("Has the following properties for " + prediacteName,15);
            AddLabel(property);
          }
          if (isPropery)
          {
            AddLabel("Is property " + prediacteName + " of following subjects:",15);
            AddLabel(isOf);
          }
          if (numAdded > 10)
          {
            break;
          }
        }
      });
    });

  }

  // Update is called once per frame
  void Update()
  {

  }

  public GameObject AddLabel(string labelText, float fontSize = 10)
  {
    GameObject label = Instantiate<GameObject>(labelPrefab);
    label.transform.SetParent(ContentPanel.transform, false);
    label.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = labelText;
    label.GetComponentInChildren<TMPro.TextMeshProUGUI>().fontSize = fontSize;
    labels.Add(label);
    return label;
  }
}
