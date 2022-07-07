using UnityEngine;

public class NailRotation : MonoBehaviour
{
   public Graph graph;
   public Vector3 rotation;
   public float offset = 0.2f;
   // Start is called before the first frame update
   void Start()
   {
      graph = transform.parent.GetComponent<Node>().graph;
   }

   // Update is called once per frame
   void Update()
   {
      transform.rotation = graph.transform.rotation * Quaternion.Euler(rotation);
      transform.localPosition = transform.localRotation * new Vector3(0, 0, offset);
   }
}
