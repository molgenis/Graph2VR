using Dweiss;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : BaseMenu
{
   public bool lastMenuButtonState = false;
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

   }

}
