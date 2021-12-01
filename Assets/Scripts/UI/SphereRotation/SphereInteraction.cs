using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class SphereInteraction : MonoBehaviour
{
  public SteamVR_Action_Boolean gripAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabGrip");
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
    bool zoomAction = gripAction.GetState(SteamVR_Input_Sources.LeftHand) && gripAction.GetState(SteamVR_Input_Sources.RightHand);

    if (!isActive && zoomAction)
    {
      closest = FindClosestGraph();
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

  private GameObject FindClosestGraph()
  {
    GameObject closest = null;
    GameObject[] graphs = GameObject.FindGameObjectsWithTag("Graph");
    float closestDistance = float.MaxValue;
    foreach (GameObject graph in graphs)
    {
      float distance = Vector3.Distance(leftController.position, graph.GetComponent<Graph>().boundingSphere.transform.position);
      if (distance < closestDistance)
      {
        closestDistance = distance;
        closest = graph;
      }
    }
    return closest;
  }


}
