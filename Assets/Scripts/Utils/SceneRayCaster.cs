using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SceneRayCaster : BaseRaycaster
{
   public float maxDistance = 10;
   public LayerMask layer;
   public GameObject leftControlerModel;
   public override Camera eventCamera
   {
      get
      {
         return GetComponent<Camera>();
      }
   }

   public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
   {
      Ray ray = new Ray(gameObject.transform.position, gameObject.transform.forward);
      RaycastHit[] hitResults = Physics.RaycastAll(ray, maxDistance, layer.value);
      foreach (RaycastHit hitResult in hitResults)
      {
         if (hitResult.collider.gameObject != null)
         {
            RaycastResult result = new RaycastResult();
            result.gameObject = hitResult.collider.gameObject;
            result.worldPosition = hitResult.point;
            result.distance = hitResult.distance;
            result.module = this;
            result.worldNormal = hitResult.normal;
            result.screenPosition = new Vector2(0, 0);

            resultAppendList.Add(result);
         }
      }
      if (hitResults.Length == 0)
      {
         if (ControlerInput.instance.triggerRight)
         {
            GameObject.FindGameObjectWithTag("LeftController").BroadcastMessage("Close", SendMessageOptions.DontRequireReceiver);
            leftControlerModel.SetActive(true);
         }
      }
   }
}
