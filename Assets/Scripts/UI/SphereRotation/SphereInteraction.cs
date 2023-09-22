﻿using UnityEngine;

public class SphereInteraction : MonoBehaviour
{
  public bool isActive = false;

  Transform leftController;
  Transform rightController;

  public Vector3 lefToRight; // In worldspace
  public Vector3 leftToCenter;
  public float handleDistance;
  Vector3 initialScale;
  Quaternion initialRotation;
  private Graph graph;

  Vector3 cachedLeft;
  Vector3 cachedRight;

  void StartInteraction()
  {
    Vector3 center = graph.boundingSphere.transform.position;
    Vector3 left = leftController.transform.position;
    Vector3 right = rightController.transform.position;

    // Correction for broken controler tracking frames
    if (left == right)
    {
      left = cachedLeft;
      right = cachedRight;
    }
    else
    {
      cachedLeft = left;
      cachedRight = right;
    }

    lefToRight = right - left;
    leftToCenter = center - left;
    handleDistance = Vector3.Distance(left, right);
    initialScale = graph.transform.localScale;
    initialRotation = graph.transform.rotation;
  }

  void StopInteraction() { }
  
  void UpdateInteraction()
  {
    Vector3 left = leftController.transform.position;
    Vector3 right = rightController.transform.position;

    // Correction for broken controler tracking frames
    if (left == right)
    {
      left = cachedLeft;
      right = cachedRight;
    }
    else
    {
      cachedLeft = left;
      cachedRight = right;
    }

    Vector3 newLefToRight = right - left;
    float sizeFactor = Vector3.Distance(left, right) / handleDistance;

    Quaternion rotation = Quaternion.FromToRotation(lefToRight, newLefToRight);
    Vector3 center = (left + ((rotation * leftToCenter) * sizeFactor)); // The calculated center ( of the bounding sphere ) after user mutation 

    graph.transform.position = center + (graph.transform.position - graph.boundingSphere.transform.position);
    graph.transform.rotation = rotation * initialRotation;
    graph.transform.localScale = initialScale * sizeFactor;

    // Reset the initial values of the rotation at 90 degree thresholds.
    if (rotation.eulerAngles.x > 90.0f && rotation.eulerAngles.x < 270.0f ||
        rotation.eulerAngles.y > 90.0f && rotation.eulerAngles.y < 270.0f ||
        rotation.eulerAngles.z > 90.0f && rotation.eulerAngles.z < 270.0f)
    {
      lefToRight = right - left;
      leftToCenter = graph.boundingSphere.transform.position - left;
      handleDistance = Vector3.Distance(left, right);
      initialScale = graph.transform.localScale;
      initialRotation = graph.transform.rotation;
    }
  }

  private void Start()
  {
    leftController = GameObject.FindGameObjectWithTag("LeftController").transform;
    rightController = GameObject.FindGameObjectWithTag("RightController").transform;
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
