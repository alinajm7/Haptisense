using UnityEngine;
using AliN.Microcontroller.Classes;

namespace AliN.Microcontroller
{
    [ExecuteInEditMode]
    public class SurfaceObjectMapper : MonoBehaviour
    {
        public enum AxisDirection
        {
            Positive_X,
            Negative_X,
            Positive_Y,
            Negative_Y,
            Positive_Z,
            Negative_Z
        }

        [Header("Mapping Parameters")]
        public GameObject MappingSourceObject;
        public AxisDirection axisDirection = AxisDirection.Negative_Y;
        

        [Header("Actuators GameObject")]
        public Actuator[] arrayOfActuators;

        public Color debugLineColor = Color.red;
        public float debugLineDuration = 2.0f;

        void Start()
        {
            //SetAllChildObjectsAndActuator();
            //MapObjectsToSurface();
        }

        public void SetAllChildObjectsAndActuator()
        {
            int childCount = transform.childCount;
            arrayOfActuators = new Actuator[childCount];

            for (int i = 0; i < childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                arrayOfActuators[i] = new Actuator(child);
            }
        }

        public void MapObjectsToSurface()
        {
            

            foreach (Actuator child in arrayOfActuators)
            {
                RaycastHit hit;
                if (Physics.Raycast(child.actuatorObject.transform.position, GetAxisFromEnum(axisDirection), out hit))
                {
                    if (hit.collider.gameObject == MappingSourceObject)
                    {
                        // // Relocate and reorient the object based on the hit point and normal
                        child.actuatorObject.transform.position = hit.point;
                        child.actuatorObject.transform.up = hit.normal;

                    }
                }
               
            }
           
        }

        public void DrawLineFromObjectsToSurface()
        {


            foreach (Actuator child in arrayOfActuators)
            {
                RaycastHit hit;
                if (Physics.Raycast(child.actuatorObject.transform.position, GetAxisFromEnum(axisDirection), out hit))
                {
                    if (hit.collider.gameObject == MappingSourceObject)
                    {
                        // Draw debug line
                        Debug.DrawLine(child.actuatorObject.transform.position, hit.point, debugLineColor, debugLineDuration);

                    }
                }

            }

        }


        private Vector3 GetAxisFromEnum(AxisDirection direction)
        {
            Vector3 axis = Vector3.zero;

            switch (direction)
            {
                case AxisDirection.Positive_X:
                    axis = Vector3.right;
                    break;
                case AxisDirection.Negative_X:
                    axis = Vector3.left;
                    break;
                case AxisDirection.Positive_Y:
                    axis = Vector3.up;
                    break;
                case AxisDirection.Negative_Y:
                    axis = Vector3.down;
                    break;
                case AxisDirection.Positive_Z:
                    axis = Vector3.forward;
                    break;
                case AxisDirection.Negative_Z:
                    axis = Vector3.back;
                    break;
            }

            return axis;
        }
    }
}
