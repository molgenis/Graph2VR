using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContextMenuHandler : MonoBehaviour
{
  public GameObject ContentPanel;
  public GameObject labelPrefab;
  InputField inputPrefab;

  // Start is called before the first frame update
  List<GameObject> labels = new List<GameObject>();
  public delegate void OnItemIsSelected(string button);


  public void Initiate(Node node)
  {
    // Todo: reimplement with the new multiple graph system
    /*
        labelPrefab = Resources.Load<GameObject>("UI/Label");
        QueryService.Instance.GetDescriptionAsync(node.uri, (graph, state) => {
            UnityMainThreadDispatcher.Instance().Enqueue( () =>{
                HashSet<INode> predicates = new HashSet<INode>();
                foreach (Triple triple in graph.Triples)
                {
                    predicates.Add(triple.Predicate);
                }
                int numAdded = 0;
                foreach (INode predicate in predicates)
                {
                    string property = "Has porperty\n" + predicate.ToString() + ":\n";
                    string isOf = "is\n" + predicate.ToString() + " of:\n";
                    IEnumerable<Triple> triples = graph.GetTriplesWithPredicate(predicate);
                    bool hasPropery = false;
                    bool isPropery = false;
                    foreach (Triple triple in triples)
                    {
                        string subject = triple.Subject.ToString();
                        string obj = triple.Object.ToString();
                        if (subject.Equals(node.uri))
                        {
                            property += obj + "\n";
                            hasPropery = true;
                            numAdded++;
                        }
                        else if (obj.Equals(node.uri))
                        {
                            isOf += subject + "\n";
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
                        AddLabel(property);
                    }
                    if (isPropery)
                    {
                        AddLabel(isOf);
                    }
                    if (numAdded > 100)
                    {
                        break;
                    }
                }
            });
        });
        */
  }

  // Update is called once per frame
  void Update()
  {

  }

  public void AddLabel(string labelText)
  {
    GameObject label = Instantiate<GameObject>(labelPrefab);
    label.transform.SetParent(ContentPanel.transform, false);
    label.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = labelText;
    labels.Add(label);
  }
}
