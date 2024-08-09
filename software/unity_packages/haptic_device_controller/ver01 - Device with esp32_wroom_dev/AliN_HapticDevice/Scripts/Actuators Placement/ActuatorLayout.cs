using System;
using UnityEngine;
using AliN.Microcontroller.Classes;

namespace AliN.Microcontroller
{
    [Serializable]
    public class exceptionActuatorPosition
    {
        public int numberOfLine = 8;
        public int NumberOfItemInRaw = 6;
    }

    public enum CurveAxis { None, X, Z }

    public class ActuatorLayout : MonoBehaviour
    {
        [Header("Actuators GameObject")]
        public Actuator[] arrayOfActuators;

        [Header("Layout Parameters")]

        public Vector3 startPosition = new Vector3(0, 0, 1.5f);
        public Vector3 offsetOnSecondLine = new Vector3(0, 0, -1.5f);
        public float inlineObjectsInterval = 3f;
        public float betweenLinesInterval = 1.5f;
        public int objectsPerOddLines = 5;
        public int objectsPerEvenLines = 6;

        public exceptionActuatorPosition[] exceptions;

        [Header("Curvature")]
        public CurveAxis curveAxis = CurveAxis.None;
        public float curveRadius = 5.0f;

        // Start is called before the first frame update
        void Start()
        {
            //// Collect all child actuator objects
            //SetAllChildObjectsAndActuator();
            //ArrangeInLayout();
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

        public void ArrangeInLayout()
        {
            if (arrayOfActuators == null)
            {
                Debug.LogError("arrayOfActuators is null.");
                return;
            }

            Vector3 position = startPosition;
            int actuatorIndex = 0;
            int lineNumber = 1;
            int objectsInLine = objectsPerOddLines;

            while (actuatorIndex < arrayOfActuators.Length)
            {
                // Apply offset only for even lines before arranging objects in that line
                if (lineNumber % 2 == 0)
                {
                    position.z += offsetOnSecondLine.z;
                }

                for (int obj = 1; obj <= objectsInLine; obj++)
                {
                    // Skip positioning for exceptions
                    bool skipPositioning = false;
                    foreach (exceptionActuatorPosition exception in exceptions)
                    {
                        if (lineNumber == exception.numberOfLine && obj == exception.NumberOfItemInRaw)
                        {
                            skipPositioning = true;
                            break;
                        }
                    }

                    if (skipPositioning)
                    {
                        continue;
                    }

                    // Position the object
                    if (actuatorIndex < arrayOfActuators.Length)
                    {
                        arrayOfActuators[actuatorIndex].actuatorObject.transform.localPosition = position;
                        // Reset the rotation
                        arrayOfActuators[actuatorIndex].actuatorObject.transform.rotation = Quaternion.identity;
                        actuatorIndex++;
                    }
                    else
                    {
                        return;
                    }

                    position.z += inlineObjectsInterval;
                }

                // Reset z-position to the starting point
                position.z = startPosition.z;

                // Update x-position for the next line
                position.x += betweenLinesInterval;

                // Update object count for the next line
                objectsInLine = (lineNumber % 2 == 0) ? objectsPerOddLines : objectsPerEvenLines;

                lineNumber++;
            }
        }



    }
}
