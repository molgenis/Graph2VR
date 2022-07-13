using UnityEngine;

public class SelectControllerModel : MonoBehaviour
{
  public bool isLeftHandedModel = false;
  public GameObject questLeft;
  public GameObject questRight;
  public GameObject vive;
  public bool switchHands = false;
  private string modelName = "quest";
  private GameObject leftModel = null;
  private GameObject rightModel = null;

  public void UpdateLeftRightHandedInterface()
  {
    switchHands = PlayerPrefs.GetInt("isLeftHanded", 0) == 1;
    SwitchModels();
  }

  private void SwitchModels()
  {
    if (leftModel != null) Destroy(leftModel);
    if (rightModel != null) Destroy(rightModel);

    if (modelName == "quest")
    {
      if (isLeftHandedModel ^ switchHands)
      {
        leftModel = Instantiate(questLeft, transform);
      }
      else
      {
        rightModel = Instantiate(questRight, transform);
      }
    }
    else
    {
      leftModel = Instantiate(vive, transform);
    }
  }

  private void Start()
  {
    UpdateLeftRightHandedInterface();

    ControllerType.instance.GetControllerName((string name) =>
    {
      modelName = name;
      SwitchModels();
    });
  }
}
