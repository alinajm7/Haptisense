using UnityEngine;
using System.Collections.Generic;
using AliN.Microcontroller.Classes;
using System;

namespace AliN.Microcontroller
{
    //[RequireComponent(typeof(ActuatorDataSender))]
    public class ActuatorsManager : MonoBehaviour
    {
        [Header("General Setting")]
        //public string tangibleObjectLayerName = "Tangible";
        //public  TangibleObject[] materialTags;
        public bool distanceToClosestSurface = true;
        public bool colorChangeByDistance = true;
        public bool drawingDistanceLine = true;
        public bool ShowActuatorValues = true;
        public int actuatorValueForntSize = 10;
        public bool ShowActuatorMoreData = true;
        public Gradient gradient;

        [Header("Behaviour Setting")]
        public float scaningDistance = 10f;
        public float powerFactor = 2.0f;
        [Range(0, 1f)]
        public float acuatorMinValueThreshold = 0.14f;
        [Range(0, 1f)]
        public float maxActuatorValue = 1.0f;

        [Header("Number of Affecting Tangible objects and Distance Base Weights Setting")]
        [Tooltip("Defines the influence of each tangible object on the actuator based on its distance. The first value is the weight for the closest object, the second value is for the second closest object, and so on.")]
        public float[] distanceBaseWeights = { 1.0f, 0.5f, 0.25f };

        [Header("Actuators GameObject")]
        public Actuator[] arrayOfActuators;
        public TangibleObject[] tangibleObjects;

        private HashSet<TangibleObject> tangibleObjectSet = new HashSet<TangibleObject>();

        // Initialize actuators and Tangible objects in the scene
        void Start()
        {
            // Collect all child actuator objects
            SetAllChildObjectsAndActuator();
            // Collect all Tangible objects in the scene
            FindAllTangibleObjectsInTheScene();

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
                actuator.ResetActuatorProperties();
                // Update actuator properties based on scene objects
                UpdateActuatorState(actuator);
            }
        }

        private void UpdateActuatorState(Actuator actuator)
        {
            int numberOfTangibleObjects = tangibleObjectSet.Count;

            float[] distances = new float[numberOfTangibleObjects];
            Vector3[] points = new Vector3[numberOfTangibleObjects];
            TangibleObject[] tangibleObjectsTemp = new TangibleObject[numberOfTangibleObjects];
            
            int index = 0;
            foreach (TangibleObject obj in tangibleObjectSet)
            {
                float distance = CalculateDistance(actuator, obj.gameObject, out Vector3 point);
                distances[index] = distance;
                points[index] = point;
                tangibleObjectsTemp[index] = obj;               
                index++;
            }

            // Call a method to calculate the actuator value and state durations
            CalculateActuatorValues(actuator, distances, points, tangibleObjectsTemp);
        }
       

        private void CalculateActuatorValues(Actuator actuator, float[] distances, Vector3[] points, TangibleObject[] tangibleObjectsTempIn)
        {
            if (distances.Length > 0)
            {
                // Sort the distances and keep track of the original indices
                int[] indices = new int[distances.Length];
                for (int i = 0; i < distances.Length; i++)
                    indices[i] = i;

                Array.Sort(distances, indices);

                // Count the number of distances within the scanning distance
                int countWithinScan = 0;
                for (int i = 0; i < distances.Length; i++)
                {
                    if (distances[i] <= scaningDistance)
                    {
                        countWithinScan++;
                    }
                }

                if (countWithinScan > 0)
                {
                    // Create arrays for filtered distances and points
                    float[] filteredValue = new float[countWithinScan];
                    Vector3[] filteredPoints = new Vector3[countWithinScan];
                    int[] filteredIndices = new int[countWithinScan];
                    TangibleObject[] filteredTangibleObjects = new TangibleObject[countWithinScan];

                    // Populate the filtered arrays
                    int filteredIndex = 0;
                    for (int i = 0; i < distances.Length; i++)
                    {
                        if (distances[i] <= scaningDistance)
                        {
                            float minvalue = acuatorMinValueThreshold;
                            float maxValue = maxActuatorValue;

                            if (tangibleObjectsTempIn[i].useLocalMaxValue) maxValue = tangibleObjectsTempIn[i].tangibleMaxValue;
                            if (tangibleObjectsTempIn[i].useLocalMinThreshold) minvalue = tangibleObjectsTempIn[i].tangibleMinValueThreshold;


                            float tmp = Mathf.Pow(Mathf.Clamp01(1 - (distances[i] / scaningDistance)), powerFactor);
                            
                           
                            tmp = tmp > maxValue ? maxValue : tmp;
                            tmp = tmp < minvalue ? 0f : tmp;

                            filteredValue[filteredIndex] = tmp;
                            filteredPoints[filteredIndex] = points[indices[i]];
                            filteredIndices[filteredIndex] = indices[i];
                            filteredTangibleObjects[filteredIndex] = tangibleObjectsTempIn[i];
                            filteredIndex++;
                        }
                    }
                    CalculateFinalActuatorValueAndLowHighStates(actuator, filteredValue, filteredPoints, filteredTangibleObjects);
                }
            }

        }

        private void CalculateFinalActuatorValueAndLowHighStates(Actuator actuator, float[] filteredValue, Vector3[] filteredPoints, TangibleObject[] filteredTangibleObjects)
        {
            // My algorihtm for calculating the parameters

            float totalActuatorValueTmp = 0;
            float totalLowStateDurationTmp = 0;
            float totalHighStateDurationTmp = 0;

            for (int i = 0; i < filteredValue.Length && i < distanceBaseWeights.Length; i++)
            {

                totalActuatorValueTmp += distanceBaseWeights[i] * filteredValue[i];

                int currentLowStateDuration, currentHighStateDuration;

                if (filteredTangibleObjects[i].useDynamicFrequency)
                {
                    float weight = distanceBaseWeights[i];
                    

                    int totalDurationFar = Mathf.RoundToInt(1000000f / filteredTangibleObjects[i].distanceScanningFrequency);
                    int totalDurationClose = Mathf.RoundToInt(1000000f / filteredTangibleObjects[i].proximityDetectionFrequency);

                    // Calculate the step duration based on the weight and the influence of both frequencies
                    int stepDuration = totalDurationFar + Mathf.RoundToInt(weight * filteredValue[i] * (totalDurationClose - totalDurationFar));

                    currentLowStateDuration= stepDuration;
                    currentHighStateDuration= stepDuration;
                }
                else
                {
                    int totalDuration = Mathf.RoundToInt(1000000f / (filteredTangibleObjects[i].initialFrequencyInHz));
                    //  Here I keep the total duration of the cycle fixed and changing the proportion of low and high dtate duration.
                    currentLowStateDuration = (int)(totalDuration - (distanceBaseWeights[i] * filteredValue[i] * totalDuration));
                    currentHighStateDuration = (int)(distanceBaseWeights[i] * filteredValue[i] * totalDuration);
                }

                if (filteredTangibleObjects[i].useFixedLowTimeFrequency) { currentLowStateDuration = filteredTangibleObjects[i].fixedLowTimeFrequency; }

                if (filteredTangibleObjects[i].useFixedHighTimeFrequency) { currentHighStateDuration = filteredTangibleObjects[i].fixedHighTimeFrequency; }

                totalLowStateDurationTmp += currentLowStateDuration;
                totalHighStateDurationTmp += currentHighStateDuration;

            }

           
            actuator.actuatorValue = Mathf.Clamp01 (totalActuatorValueTmp);
            actuator.lowStateDuration = (int)(Mathf.Clamp(totalLowStateDurationTmp, 1,99999)); // lowStateDuration is Clamp to 1 because if it is 0 microcontroller turnoff the actuator
            /*  To prevent the microcontroller from turning off the actuator when the low duration is zero, the `lowStateDuration` is clamped to a minimum value of `1`. This ensures that the actuator remains active even with very short durations, while values close to zero but not exactly zero are sent for correct functioning. The `highStateDuration` is clamped between `0` and `99999` to accommodate varying actuation needs.  */
            actuator.highStateDuration = (int)(Mathf.Clamp(totalHighStateDurationTmp, 0, 99999));

            //if (actuator.highStateDuration != actuator.lastHighStateDuration || actuator.lowStateDuration != actuator.lastLowStateDuration)
            //{
            //    actuator.lastHighStateDuration = actuator.highStateDuration;
            //    actuator.lastLowStateDuration = actuator.lowStateDuration;
            //    Actuator.actuatorValueChanged = true;  // if value changed gives indication to send data to  Microcontroller
            //}
           
            // Display Visual representations
            VisualPresentationUpdate(actuator, filteredValue, filteredPoints);
        }

        private void VisualPresentationUpdate(Actuator actuator, float[] filteredValue, Vector3[] filteredPoints)
        {
            // Color & Draw lines for all affecting tangible objects
            if (actuator.actuatorValue != 0)
            {
                if (colorChangeByDistance)
                {
                    actuator.material.color = gradient.Evaluate(actuator.actuatorValue);
                }

                if (drawingDistanceLine)
                {
                    for (int i = 0; i < filteredValue.Length && i < distanceBaseWeights.Length; i++)
                    {
                        Vector3 point = filteredPoints[i];
                        Debug.DrawLine(actuator.transform.position, point, actuator.material.color);
                    }
                }
            }
        }

        // Calculate distance between the actuator and a Tangible object
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

        private void OnTriggerEnter(Collider other)
        {
            TangibleObject tangibleObject = other.GetComponent<TangibleObject>();
            if (tangibleObject != null)
            {
                tangibleObjectSet.Add(tangibleObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            TangibleObject tangibleObject = other.GetComponent<TangibleObject>();
            if (tangibleObject != null)
            {
                tangibleObjectSet.Remove(tangibleObject);
            }
        }

        // Function to remap actuator values
        private int RemapValue(float value,  float remappedMaxValue) // Assuming that input values is 0 to 1 and minimum output value is 0
        {
            float remapped = (value ) * (remappedMaxValue);
            return Mathf.RoundToInt(remapped);
        }

        //private int RemapValue(float value, float minInputValue, float maxInputValue, float remappedMinValue, float remappedMaxValue)
        //{
        //    float remapped = (value - minInputValue) / (maxInputValue - minInputValue) * (remappedMaxValue - remappedMinValue) + remappedMinValue;
        //    return Mathf.RoundToInt(remapped);
        //}

        /// <summary>
        // Set Actuators and Tangible Objects in arrays
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

        // Find relevant Objects in the scene. Relevant objects are those with specific component "TangibleObject"
        public void FindAllTangibleObjectsInTheScene()
        {


            // Convert the list to an array and assign it to tangibleObjects
            tangibleObjects = FindObjectsOfType<TangibleObject>();
        }

        /// </summary>
        /// 
    }
}
