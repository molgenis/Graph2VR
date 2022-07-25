using System;
using UnityEngine;
using VRKeys;

public class GetStringFromKeyboardHandler : MonoBehaviour
{
  private Action<string> callback;
  public Keyboard keyboard;
  public void DisplayOnlyControllerModel(bool value)
  {
    GameObject.FindGameObjectWithTag("LeftController").transform.Find("Offset").Find("Menu").gameObject.SetActive(!value);
    GameObject.FindGameObjectWithTag("LeftController").transform.Find("Offset").Find("Model").gameObject.SetActive(true);
    GameObject.FindGameObjectWithTag("LeftController").transform.Find("Offset").Find("SelectedNode").gameObject.SetActive(!value);
    GameObject.FindGameObjectWithTag("LeftController").transform.Find("Offset").Find("Sphere").gameObject.SetActive(!value);
  }

  public void GetString(Action<string> callback, string initialValue = "", string placeHolder = "...")
  {
    this.callback = callback;
    keyboard.SetText(initialValue);
    keyboard.Enable();
    keyboard.SetPlaceholderMessage(placeHolder);
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
