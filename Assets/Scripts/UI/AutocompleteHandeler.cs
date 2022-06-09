using Dweiss;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VDS.RDF;
using VDS.RDF.Query;
using VRKeys;

public class AutocompleteHandeler : MonoBehaviour
{
  public GameObject searchResultPrefab;
  public Keyboard keyboard;

  public GameObject searchResults;
  public GameObject searchResultsLayout;
  public static AutocompleteHandeler Instance;

  private Action<string, string> foundResultsCallback;

  private void Awake()
  {
    Instance = this;
  }

  private void Start()
  {
    keyboard = GetComponent<Keyboard>();
  }

  public void DisplayOnlyControllerModel(bool value)
  {
    GameObject.FindGameObjectWithTag("LeftController").transform.Find("Offset").Find("NodeMenu").gameObject.SetActive(!value);
    GameObject.FindGameObjectWithTag("LeftController").transform.Find("Offset").Find("Model").gameObject.SetActive(true);
    GameObject.FindGameObjectWithTag("LeftController").transform.Find("Offset").Find("SelectedNode").gameObject.SetActive(!value);
    GameObject.FindGameObjectWithTag("LeftController").transform.Find("Offset").Find("Sphere").gameObject.SetActive(!value);
  }

  public void SearchForNode(Action<string, string> callback)
  {
    foundResultsCallback = callback;
    keyboard.SetText("");
    ClearUIItems();
    keyboard.Enable();
    keyboard.SetPlaceholderMessage("Please enter search term");
    KeyboardHandler.instance.UpdateLocation();
    DisplayOnlyControllerModel(true);

    if (Settings.Instance.SearchOnKeypress)
    {
      keyboard.OnUpdate.AddListener(HandleSearch);
    }
    else
    {
      keyboard.OnSubmit.AddListener(HandleSearch);
    }
    keyboard.OnCancel.AddListener(HandleCancel);
  }

  private void HandleCancel()
  {
    DisplayOnlyControllerModel(false);
    keyboard.Disable();
    RemoveListeners();
  }

  private void RemoveListeners()
  {
    if (Settings.Instance.SearchOnKeypress)
    {
      keyboard.OnUpdate.RemoveListener(HandleSearch);
    }
    else
    {
      keyboard.OnSubmit.RemoveListener(HandleSearch);
    }
    keyboard.OnCancel.RemoveListener(HandleCancel);
  }

  private void HandleSearch(string searchTerm)
  {
    ClearUIItems();
    AddItem("Loading", "please wait a moment", false);
    QueryService.Instance.AutocompleteSearch(searchTerm, SearchCallback);
  }

  private void SearchCallback(SparqlResultSet results, object state)
  {
    if (results == null)
    {
      ClearUIItems();
      return;
    }
    if (state is AsyncError)
    {
      ClearUIItems();
      AddItem("Error", "Timeout", false);
      return;
    }
    UnityMainThreadDispatcher.Instance().Enqueue(() =>
    {
      ClearUIItems();
      foreach (SparqlResult result in results)
      {
        AddItem(result.Value("name").ToString(), result.Value("uri").ToString());
      }
    });
  }
  private void ClearUIItems()
  {
    foreach (Transform child in searchResultsLayout.transform)
    {
      Destroy(child.gameObject);
    }
    searchResults.SetActive(false);
  }

  private void AddItem(string name, string uri, bool clickable = true)
  {
    searchResults.SetActive(true);
    GameObject clone = Instantiate<GameObject>(searchResultPrefab);
    clone.transform.SetParent(searchResultsLayout.transform);
    clone.transform.localPosition = Vector3.zero;
    clone.transform.localRotation = Quaternion.identity;
    clone.transform.localScale = Vector3.one;

    TextMeshProUGUI text = clone.transform.Find("Text").GetComponent<TextMeshProUGUI>();
    text.text = name + " - " + uri;
    if (clickable)
    {
      Button button = clone.transform.GetComponent<Button>();
      button.onClick.AddListener(() =>
      {
        keyboard.Disable();
        RemoveListeners();
        DisplayOnlyControllerModel(false);
        string label = button.transform.Find("Text").GetComponent<TextMeshProUGUI>().text;
        foundResultsCallback(name, uri);
      });
    }
  }
}
