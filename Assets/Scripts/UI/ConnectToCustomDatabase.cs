using Dweiss;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRKeys;

public class ConnectToCustomDatabase : MonoBehaviour
{
  private Action<string> callback;
  public Keyboard keyboard;
  public void DisplayOnlyControllerModel(bool value)
  {
    GameObject.FindGameObjectWithTag("LeftController").transform.Find("Offset").Find("NodeMenu").gameObject.SetActive(!value);
    GameObject.FindGameObjectWithTag("LeftController").transform.Find("Offset").Find("Model").gameObject.SetActive(true);
    GameObject.FindGameObjectWithTag("LeftController").transform.Find("Offset").Find("SelectedNode").gameObject.SetActive(!value);
    GameObject.FindGameObjectWithTag("LeftController").transform.Find("Offset").Find("Sphere").gameObject.SetActive(!value);
  }

  public void GetEndpoint(Action<string> callback)
  {
    this.callback = callback;
    keyboard.SetText("");
    keyboard.Enable();
    keyboard.SetPlaceholderMessage("Please enter search term");
    KeyboardHandler.instance.UpdateLocation();
    DisplayOnlyControllerModel(true);

    keyboard.OnSubmit.AddListener(HandleSubmit);
    keyboard.OnCancel.AddListener(HandleCancel);
  }

  private void HandleCancel()
  {
    keyboard.SetText("");
    DisplayOnlyControllerModel(false);
    keyboard.Disable();
    RemoveListeners();
  }

  private void RemoveListeners()
  {
    keyboard.OnUpdate.RemoveAllListeners();
    keyboard.OnSubmit.RemoveAllListeners();
    keyboard.OnCancel.RemoveAllListeners();
  }

  private void HandleSubmit(string searchTerm)
  {
    DisplayOnlyControllerModel(false);
    keyboard.Disable();
    keyboard.SetText("");
    RemoveListeners();
    callback(searchTerm);
  }

}
