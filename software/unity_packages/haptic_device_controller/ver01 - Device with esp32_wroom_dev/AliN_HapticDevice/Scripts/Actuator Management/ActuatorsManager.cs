using UnityEngine;
using System.Collections.Generic;
using AliN.Microcontroller.Classes;

namespace AliN.Microcontroller
{
    [RequireComponent(typeof(ActuatorDataSender))]
    public class ActuatorsManager : MonoBehaviour
    {
        [Header("General Setting")]
        public string hapticObjectLayerName = "Haptable";
        public bool distanceToClosestSurface = true;
        public bool colorChangeByDistance = true;
        public bool drawingDistanceLine = true;
        public bool ShowActuatorValues = true;
        public Gradient gradient;


        [Header("Behaviour Setting")]
        public float scaningDistance = 10f;
        public float powerFactor = 2.0f;
        [Range(0, 1)]
        public float acuatorMinValueThreshold = 0.14f;
        [Range(0, 1)]
        public float maxActuatorValue = 1.0f; 


        [Header("Actuators GameObject")]
        public Actuator[] arrayOfActuators;
        public GameObject[] haptableObjects;


        private BoxCollider rangeCollider;
        // Start is called before the first frame update
        // Initialize actuators and haptable objects in the scene
        void Start()
        {
            // Collect all child actuator objects
            SetAllChildObjectsAndActuator();
            // Collect all haptable objects in the scene
            FindAllHaptableObjectsInTheScene();
            rangeCollider = gameObject.GetComponent<BoxCollider>();
        }

        // Update is called once per frame
        // Refresh the state of all actuators
        void Update()
        {
            UpdateActuators();
        }

        // Reset and update each actuator in the scene
        private void UpdateActuators()
        {
            foreach (var actuator in arrayOfActuators)
            {
                // Reset actuator to its initial state
                ResetActuator(actuator);
                // Update actuator properties based on scene objects
                UpdateActuatorState(actuator);
            }
        }

        // Reset the given actuator to its initial state
        private void ResetActuator(Actuator actuator)
        {
            actuator.actuatorValue = 0;
            actuator.material.color = actuator.originalColor;
        }

        // Update the state of a single actuator
        private void UpdateActuatorState(Actuator actuator)
        {
            float closestDistance = scaningDistance;
            GameObject closestObject = null;
            Vector3 closestPoint = Vector3.zero;

            // Get Objects in layer "Haptable" and inside the boxCollider of this object
            Collider[] collidersInRange = Physics.OverlapBox(rangeCollider.center, rangeCollider.size / 2, rangeCollider.transform.rotation, LayerMask.GetMask(hapticObjectLayerName));

            foreach (var ccollider in collidersInRange) // Iterate over GameObjects
            {
                GameObject colliderGameObject = ccollider.gameObject;

                if (rangeCollider.bounds.Contains(ccollider.transform.position))
                {
                    float distance = CalculateDistance(actuator, colliderGameObject, out Vector3 point);
                    if (IsWithinScanningRange(distance, ref closestDistance, ref closestObject, ref closestPoint, point, colliderGameObject))
                    {
                        UpdateActuatorProperties(actuator, closestObject, closestPoint, closestDistance);
                    }                   
                }
            }
        }

        // Calculate distance between the actuator and a haptable object
        private float CalculateDistance(Actuator actuator, GameObject gameObject, out Vector3 point)
        {
            Collider objectCollider = gameObject.GetComponent<Collider>(); // Get the Collider

            if (distanceToClosestSurface)
            {
                MeshCollider meshCollider = objectCollider as MeshCollider;
                if (meshCollider != null && !meshCollider.convex)
                {
                    Debug.LogWarning(gameObject.name + " has a non-convex MeshCollider. Convex option should be true.");
                    point = objectCollider.ClosestPointOnBounds(actuator.transform.position);
                }
                else
                {
                    point = objectCollider.ClosestPoint(actuator.transform.position);
                }
            }
            else
            {
                point = gameObject.transform.position;
            }
            return Vector3.Distance(actuator.transform.position, point);
        }

        // Check if the distance is within the scanning range and update closestObject if true
        private bool IsWithinScanningRange(float distance, ref float closestDistance, ref GameObject closestObject, ref Vector3 closestPoint, Vector3 point, GameObject gameObject)
        {
            if (distance < closestDistance && distance < scaningDistance)
            {
                closestDistance = distance;
                closestObject = gameObject;
                closestPoint = distanceToClosestSurface ? point : gameObject.transform.position;
                return true;
            }
            return false;
        }

        // Update various properties of an actuator based on the closest object
        private void UpdateActuatorProperties(Actuator actuator, GameObject closestObject, Vector3 closestPoint, float closestDistance)
        {
            actuator.targetObjectPoint = closestPoint;

            // Apply power factor to the actuator value
            actuator.actuatorValue = Mathf.Pow(Mathf.Clamp01(1 - (closestDistance / scaningDistance)), powerFactor);

            actuator.actuatorValue = actuator.actuatorValue > maxActuatorValue ? maxActuatorValue : actuator.actuatorValue;
            actuator.actuatorValue = actuator.actuatorValue < acuatorMinValueThreshold ? 0f : actuator.actuatorValue;

            if (actuator.actuatorValue != 0)
            {
                if (colorChangeByDistance)
                {
                    actuator.material.color = gradient.Evaluate(actuator.actuatorValue);
                }

                if (drawingDistanceLine)
                {
                    Debug.DrawLine(actuator.transform.position, closestPoint, actuator.material.color);
                }
            }
        }

        /// <summary>
        // Set Actuators and haptable Objects in arrays
        // Initialize arrayOfActuators with child GameObjects of the current GameObject
        public void SetAllChildObjectsAndActuator()
        {
            int childCount = transform.childCount;

            // Initialize the array with the number of child objects
            arrayOfActuators = new Actuator[childCount];

            for (int i = 0; i < childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                arrayOfActuators[i] = new Actuator(child);
            }
        }

        // Find relevant Objects in the scene. Relevant objects are those in specific Layer "hapticObjectLayerName"
        public void FindAllHaptableObjectsInTheScene()
        {
            // Convert layer name to layer number
            int hapticObjectLayer = LayerMask.NameToLayer(hapticObjectLayerName);

            // Temporary list to store matching objects
            List<GameObject> objectsInLayer = new List<GameObject>();

            // Get all GameObjects in the scene
            GameObject[] allObjects = FindObjectsOfType<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                // Check if the object's layer matches the hapticObjectLayer
                if (obj.layer == hapticObjectLayer)
                {
                    objectsInLayer.Add(obj);
                }
            }

            // Convert the list to an array and assign it to haptableObjects
            haptableObjects = objectsInLayer.ToArray();
        }

        /// </summary>
        /// 


    }
}
