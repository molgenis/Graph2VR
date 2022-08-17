using UnityEngine;
using UnityEngine.UI;

public class HelpMenu : MonoBehaviour
{
  public Toggle toggle;
  public GameObject container;

  private static string helpkey = "showHelpOnStartUp";

  void Start()
  {
    toggle.onValueChanged.AddListener(toggleListener);
    int savedStatus = PlayerPrefs.GetInt(helpkey, 1);
    toggle.isOn = savedStatus == 1;
    container.SetActive(toggle.isOn);
  }

  private void toggleListener(bool isChecked)
  {
    int statusAsInt = isChecked ? 1 : 0;
    PlayerPrefs.SetInt(helpkey, statusAsInt);
  }
}
