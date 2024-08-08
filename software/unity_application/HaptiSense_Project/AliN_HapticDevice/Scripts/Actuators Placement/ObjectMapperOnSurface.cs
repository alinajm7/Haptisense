using UnityEngine;
using AliN.Microcontroller.Classes;
using System.Collections;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace AliN.Microcontroller
{
    [ExecuteInEditMode]
    public class ObjectMapperOnSurface : MonoBehaviour
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
        
        public int referenceObjectIndex = 0;


        [Header("Actuators GameObject")]
        public Actuator[] arrayOfActuators;

        public Color debugLineColor = Color.green;
        public float debugLineDuration = 2.0f;

        private Vector3[] initialRelativePositions; // Store relative positions from the first actuator


        void Start()
        {
            // Using Inspector/UI-editor to run this class
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
                // Make sure to slightly offset the start position to avoid starting inside a collider
                Vector3 rayStart = child.actuatorObject.transform.position + new Vector3(0, 0.1f, 0);

                // Perform the raycast
                if (Physics.Raycast(rayStart, GetAxisFromEnum(axisDirection), out hit))
                {
                    // Check if the hit object is the MappingSourceObject
                    if (hit.collider.gameObject == MappingSourceObject)
                    {
                        // Draw debug line when hit is successful and it's the correct object
                        Debug.DrawLine(child.actuatorObject.transform.position, hit.point, debugLineColor, debugLineDuration);
                    }
                    else
                    {
                        // Draw a red line downwards if the raycast hits something else                        
                        float defaultLineLength = 5.0f; // Adjust this length as necessary
                        Debug.DrawLine(child.actuatorObject.transform.position, child.actuatorObject.transform.position + GetAxisFromEnum(axisDirection) * defaultLineLength, Color.red, debugLineDuration);
                    }
                }
                else
                {
                    // Draw a red line downwards if the raycast doesn't hit anything
                    
                    float defaultLineLength = 5.0f; // Adjust this length as necessary
                    Debug.DrawLine(child.actuatorObject.transform.position, child.actuatorObject.transform.position + GetAxisFromEnum(axisDirection) * defaultLineLength, Color.red, debugLineDuration);
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


        public void SaveInitialVariationWithFirstActuator()
        {
            SetAllChildObjectsAndActuator();

            if (arrayOfActuators.Length == 0)
                return;

            Vector3 firstActuatorPosition = arrayOfActuators[referenceObjectIndex].transform.position; // Get the position of the first actuator

            initialRelativePositions = new Vector3[arrayOfActuators.Length];

            // Calculate and store the relative position from the first actuator to each other actuator
            for (int i = 0; i < arrayOfActuators.Length; i++)
            {
                initialRelativePositions[i] = arrayOfActuators[i].transform.position - firstActuatorPosition;
            }


        }


        public void PlaceAllObjectsOnTheSurfaceAccordingToTheReferenceObject()
        {
            SaveInitialVariationWithFirstActuator();

            if (arrayOfActuators.Length < 2)
                return;

            RaycastHit hit;


            //Adjust the raycast origin slightly outside the original position to ensure it's not starting inside the geometry.
            Vector3 raycastOrigin = arrayOfActuators[referenceObjectIndex].transform.position + GetAxisFromEnum(axisDirection) * 0.1f;
            if (Physics.Raycast(raycastOrigin, GetAxisFromEnum(axisDirection), out hit, Mathf.Infinity))
            {
                if (hit.collider.gameObject == MappingSourceObject)
                {
                    arrayOfActuators[referenceObjectIndex].actuatorObject.transform.position = hit.point;
                    arrayOfActuators[referenceObjectIndex].actuatorObject.transform.up = hit.normal;
                }
                else
                {
                    Debug.LogError($"Raycast did not hit the expected MappingSourceObject, instead hit: {hit.collider.gameObject.name}");
                    return;  // Exit the function as the reference object did not hit the mapping source object
                }
            }
            else
            {
                Debug.LogError("Raycast from referenceObject did not hit any object.");
                return;  // Exit the function if no hit occurred
            }


            Vector3 firstActuatorPosition = arrayOfActuators[referenceObjectIndex].actuatorObject.transform.position;
            Vector3 firstActuatorNormal = hit.normal;

            // Process each subsequent actuator
            for (int i = 0; i < arrayOfActuators.Length; i++)
            {
                if (i != referenceObjectIndex)
                {
                    float pathDistance = (initialRelativePositions[i] - initialRelativePositions[referenceObjectIndex]).magnitude;  // Use the saved initial distance
                    Vector3 direction = (initialRelativePositions[i] - initialRelativePositions[referenceObjectIndex]).normalized; // Use the saved initial direction

                    Vector3 currentPosition = firstActuatorPosition;
                    Vector3 currentNormal = firstActuatorNormal;

                    // Calculate step size based on pathDistance
                    int numSteps = (int)Mathf.Ceil(pathDistance / 0.01f); // Number of steps can be adjusted
                    float stepSize = pathDistance / numSteps; // Adjust the actual step size to evenly distribute along path

                    for (int step = 0; step < numSteps; step++)
                    {
                        Vector3 stepDirection = Vector3.ProjectOnPlane(direction, currentNormal);
                        Vector3 nextStepPosition = currentPosition + stepDirection * stepSize;

                        // Raycast to ensure the step lands on the surface
                        if (Physics.Raycast(nextStepPosition + currentNormal * 0.01f, -currentNormal, out hit, stepSize + 0.01f))
                        {
                            currentPosition = hit.point;
                            currentNormal = hit.normal;
                        }
                        else
                        {
                            break; // Stop if no surface is found
                        }
                    }


                    // Set the actuator's position and orientation
                    arrayOfActuators[i].actuatorObject.transform.position = currentPosition;
                    arrayOfActuators[i].actuatorObject.transform.up = currentNormal;
                }
            }
        }



    }


}
