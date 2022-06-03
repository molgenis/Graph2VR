using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SphereInteraction : MonoBehaviour
{
  public bool isActive = false;

  Transform leftController;
  Transform rightControler;

  Vector3 lefToRight; // In worldspace
  Vector3 leftToCenter;
  float handleDistance;
  Vector3 initialScale;
  Quaternion initialRotation;
  private Graph graph;

  void StartInteraction()
  {
    Vector3 center = graph.boundingSphere.transform.position;
    Vector3 left = leftController.transform.position;
    Vector3 right = rightControler.transform.position;
    lefToRight = right - left;
    leftToCenter = center - left;
    handleDistance = Vector3.Distance(left, right);
    initialScale = graph.transform.localScale;
    initialRotation = graph.transform.rotation;
  }

  void StopInteraction()
  {

  }

  void UpdateInteraction()
  {
    Vector3 left = leftController.transform.position;
    Vector3 right = rightControler.transform.position;

    Vector3 newLefToRight = right - left;
    float sizeFactor = Vector3.Distance(left, right) / handleDistance;

    Quaternion rotation = Quaternion.FromToRotation(lefToRight, newLefToRight);
    Vector3 center = (left + ((rotation * leftToCenter) * sizeFactor));

    graph.transform.position = center + (graph.transform.position - graph.boundingSphere.transform.position);
    graph.transform.rotation = rotation * initialRotation;
    graph.transform.localScale = initialScale * sizeFactor;

    // Reset the initial values of the rotation at 90 degree thresholds.
    if (rotation.eulerAngles.x > 90.0f && rotation.eulerAngles.x < 270.0f ||
        rotation.eulerAngles.y > 90.0f && rotation.eulerAngles.y < 270.0f ||
        rotation.eulerAngles.z > 90.0f && rotation.eulerAngles.z < 270.0f)
    {
      lefToRight = right - left;
      graph.boundingSphere.Update();
      leftToCenter = graph.boundingSphere.transform.position - left;
      handleDistance = Vector3.Distance(left, right);
      initialScale = graph.transform.localScale;
      initialRotation = graph.transform.rotation;
    }
  }

  private void Start()
  {
    leftController = GameObject.FindGameObjectWithTag("LeftController").transform;
    rightControler = GameObject.FindGameObjectWithTag("RightControler").transform;
  }

  void Update()
  {
    GameObject closest = null;
    bool zoomAction = ControlerInput.instance.gripLeft && ControlerInput.instance.gripRight;

    if (!isActive && zoomAction)
    {
      closest = Utils.FindClosestGraph(leftController.position);
    }

    if (!isActive && zoomAction)
    {
      if (closest != null)
      {
        isActive = true;
        graph = closest.GetComponent<Graph>();
        StartInteraction();
      }
    }
    if (isActive && !zoomAction)
    {
      StopInteraction();
      graph = null;
      isActive = false;
    }
    if (isActive)
    {
      UpdateInteraction();
    }
  }



}
