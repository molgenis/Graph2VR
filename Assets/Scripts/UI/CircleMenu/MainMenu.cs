using Dweiss;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : BaseMenu
{
  public bool lastMenuButtonState = false;
  private bool isQuestController = true;

  public void Start()
  {
    base.Start();
    ControllerType.instance.GetControllerName((string name) =>
    {
      isQuestController = name == "quest";
    });
  }

  public override void Update()
  {
    base.Update();
    bool currentMenuButtonState = ControlerInput.instance.menu;
    if (!lastMenuButtonState && currentMenuButtonState)
    {
      GameObject.FindGameObjectWithTag("LeftController").BroadcastMessage("PopulateMainMenu", SendMessageOptions.DontRequireReceiver);
    }
    lastMenuButtonState = ControlerInput.instance.menu;
  }

  public void PopulateMainMenu()
  {
    KeyboardHandler.instance.Close();
    controlerModel.SetActive(false);
    cm.Close();
    limitSlider.SetActive(false);

    if (subMenu != "")
    {
      PopulateMainDisplaySubMenus();
    }
    else
    {
      PopulateBaseMainMenu();
    }
    cm.ReBuild();
  }

  public void PopulateMainDisplaySubMenus()
  {
    // We are in a sub menu
    cm.AddButton("Back", Color.blue / 2, () =>
    {
      subMenu = "";
      populateMenuState = PopulateMenuState.unloaded;
      cm.Close();
      PopulateMainMenu();
    });

    if (subMenu == "Settings")
    {
      PopulateSettingsMenu();
    }
  }

  public void PopulateBaseMainMenu()
  {

    cm.AddButton("Reset Graph2VR DEMO - Mountain", Color.red, () =>
    {
      foreach (GameObject gameObject in FindObjectsOfType<GameObject>())
      {
        gameObject.SetActive(false);
        Destroy(gameObject);
      }
      SceneManager.LoadScene("Main");
      return;
    });

    cm.AddButton("Reset Graph2VR DEMO - Local", Color.red, () =>
    {
      foreach (GameObject gameObject in FindObjectsOfType<GameObject>())
      {
        gameObject.SetActive(false);
        Destroy(gameObject);
      }
      GameObject clone = new GameObject("SelectCustomDatabase");
      clone.tag = "UseCustomDatabase";
      DontDestroyOnLoad(clone);
      SceneManager.LoadScene("Main");
      return;
    });
    cm.AddButton("Settings", Color.yellow / 2, () =>
    {
      subMenu = "Settings";
      cm.Close();
      PopulateMainMenu();
    });

    cm.AddButton("Save closest Graph (experimental)", new Color(1, 0.5f, 0.5f) / 2, () =>
    {
      SaveLoad.Save(Main.instance.FindClosestGraphOrCreateNewGraph(transform.position), "graph");
      Close();
    });

    cm.AddButton("Load last saved Graph (experimental)", new Color(1, 0.5f, 0.5f) / 2, () =>
    {
      Graph graph = Main.instance.CreateGraph();
      SaveLoad.Load(graph, "graph");
      graph.layout.CalculateLayout();
      Close();
    });

    cm.AddButton("Search for existing node", Color.blue / 2, () =>
    {
      Main.instance.FindClosestGraphOrCreateNewGraph(transform.position).AddNodeFromDatabase();
      Close();
    });
  }

  public void PopulateSettingsMenu()
  {
    cm.AddButton("Connect to custom server", Color.green / 2, () =>
    {
      Main.instance.GetComponent<ConnectToCustomDatabase>().GetEndpoint(endpoint =>
        {
          PlayerPrefs.SetString("CustomServer", endpoint);
          Settings.Instance.baseURI = "https://github.com/PjotrSvetachov/GraphVR/example-graph";
          Settings.Instance.sparqlEndpoint = endpoint;
          Settings.Instance.databaseSuportsBifContains = false;
          Settings.Instance.searchOnKeypress = false;
          QueryService.Instance.SwitchEndpoint();
        });
      Close();
    });

    foreach (DatabaseSetttings dataBaseSettings in Settings.Instance.databaseSetttings)
    {
      cm.AddButton("Switch to " + dataBaseSettings.label, Color.green / 2, () =>
      {
        Settings.Instance.baseURI = dataBaseSettings.baseURI;
        Settings.Instance.sparqlEndpoint = dataBaseSettings.sparqlEndpoint;
        Settings.Instance.databaseSuportsBifContains = dataBaseSettings.databaseSuportsBifContains;
        Settings.Instance.searchOnKeypress = dataBaseSettings.searchOnKeypress;
        QueryService.Instance.SwitchEndpoint();
        Close();
      });
    }

    cm.AddButton(Settings.Instance.searchOnKeypress ? "Use: Search on submit" : "Use: Search on key-press", Color.yellow / 2, () =>
    {
      Settings.Instance.searchOnKeypress = !Settings.Instance.searchOnKeypress;
      cm.Close();
      PopulateMainMenu();
    });

    if (isQuestController)
    {
      bool isLeftHanded = PlayerPrefs.GetInt("isLeftHanded", 0) == 1;
      cm.AddButton(isLeftHanded ? "Switch to righthanded" : "Switch to lefthanded", Color.yellow / 2, () =>
      {
        PlayerPrefs.SetInt("isLeftHanded", !isLeftHanded ? 1 : 0);
        GameObject.FindGameObjectWithTag("LeftController").GetComponent<SwitchHandTracking>().UpdateLeftRightHandedInterface();
        GameObject.FindGameObjectWithTag("RightController").GetComponent<SwitchHandTracking>().UpdateLeftRightHandedInterface();
        GameObject.FindGameObjectWithTag("LeftController").transform.Find("Offset").Find("Model").GetComponent<SelectControllerModel>().UpdateLeftRightHandedInterface();
        GameObject.FindGameObjectWithTag("RightController").transform.Find("Offset").Find("Model").GetComponent<SelectControllerModel>().UpdateLeftRightHandedInterface();
        GameObject.FindGameObjectWithTag("LeftController").transform.Find("Offset").GetComponent<GraphInteract>().UpdateLeftRightHandedInterface();
        GameObject.FindGameObjectWithTag("RightController").transform.Find("Offset").GetComponent<GraphInteract>().UpdateLeftRightHandedInterface();
        Transform pointer = GameObject.FindGameObjectWithTag("RightController").transform.Find("Offset").Find("Pointer").transform;
        Transform sphere = GameObject.FindGameObjectWithTag("LeftController").transform.Find("Offset").Find("Sphere").transform;
        Transform menu = GameObject.FindGameObjectWithTag("LeftController").transform.Find("Offset").Find("Menu").transform;
        Transform selectedEdge = GameObject.FindGameObjectWithTag("LeftController").transform.Find("Offset").Find("SelectedEdge").transform;
        Transform selectedNode = GameObject.FindGameObjectWithTag("LeftController").transform.Find("Offset").Find("SelectedNode").transform;
        pointer.localPosition = new Vector3(-pointer.localPosition.x, pointer.localPosition.y, pointer.localPosition.z);
        sphere.localPosition = new Vector3(-pointer.localPosition.x, pointer.localPosition.y, pointer.localPosition.z);
        menu.localPosition = new Vector3(-menu.localPosition.x, menu.localPosition.y, menu.localPosition.z);
        selectedEdge.localPosition = new Vector3(-selectedEdge.localPosition.x, selectedEdge.localPosition.y, selectedEdge.localPosition.z);
        selectedNode.localPosition = new Vector3(-selectedNode.localPosition.x, selectedNode.localPosition.y, selectedNode.localPosition.z);

        Main.instance.GetComponent<ControlerInput>().UpdateLeftRightHandedInterface();
        cm.Close();
        PopulateMainMenu();
      });
    }
  }
}
