using UnityEngine;

public class CircleMenuSliderNob : MonoBehaviour
{
   GameObject laserPointer;
   GameObject controler;
   CircleMenu menu;
   public LayerMask layerMask;

   public void Set(CircleMenu menu)
   {
      this.menu = menu;
      controler = GameObject.FindGameObjectWithTag("RightController");
      laserPointer = GameObject.FindGameObjectWithTag("LaserPointer");
   }

   // Update is called once per frame
   bool lastGrip = true;

   private float positionToSliderValue(Vector3 position)
   {
      // Calculate slider value
      Vector3 menuToControlerNormal = (position - menu.transform.position).normalized;
      Vector3 projected = Vector3.ProjectOnPlane(menuToControlerNormal, menu.transform.forward);
      return Mathf.Clamp01(Vector3.Angle(menu.transform.up, projected) / 180f);
   }

   void Update()
   {
      // Collider ray check
      RaycastHit hit;
      Collider collider = GetComponent<Collider>();
      bool actionPressed = false;
      if (ControlerInput.instance.gripRight && ControlerInput.instance.gripRight != lastGrip)
      {
         actionPressed = true;
      }

      if (ControlerInput.instance.triggerRight)
      {
         if (Physics.Raycast(new Ray(laserPointer.transform.position, laserPointer.transform.forward), out hit, 2f, layerMask.value))
         {
            menu.sliderValue = positionToSliderValue(hit.point);
         }
      }

      if (ControlerInput.instance.gripRight)
      {
         menu.sliderValue = positionToSliderValue(controler.transform.position);
      }
      else
      {
         if (collider != null)
         {
            //bool pointerSelection = collider.Raycast(new Ray(controler.transform.position, controler.transform.forward), out hit, 2f,);
            bool grabSelection = collider.Raycast(new Ray(controler.transform.position, transform.position - controler.transform.position), out hit, 0.1f);

            if (grabSelection)
            {
               // Someone is pointing at us
               gameObject.GetComponent<Renderer>().material.color = menu.defaultColor + new Color(0.2f, 0.2f, 0.2f); ;

               // Someone is clicking at us
               if (actionPressed)
               {
                  controler.transform.Find("Model").gameObject.SetActive(false);
                  controler.transform.Find("Pointer").gameObject.SetActive(false);
               }
            }
            else
            {
               gameObject.GetComponent<Renderer>().material.color = menu.defaultColor;
            }
         }
      }
      lastGrip = ControlerInput.instance.gripRight;
   }
}
