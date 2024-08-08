using System;
using UnityEngine;
using AliN.Microcontroller.Classes;

namespace AliN.Microcontroller
{
    [Serializable]
    public class exceptionActuatorPosition
    {
        public int numberOfLine = 8;
        public int numberOfItemInRaw = 6;
    }

    public enum CurveAxis { None, X, Z }

    public class ActuatorLayout : MonoBehaviour
    {
        [Header("Actuators GameObject")]
        public Actuator[] arrayOfActuators;

        // 2D array to hold the actuators based on their positions in the layout
        public Actuator[,] actuatorGrid;

        [Header("Layout Parameters")]

        public Vector3 startPosition = new Vector3(0, 0, 1.5f);
        public Vector3 offsetOnSecondLine = new Vector3(0, 0, -1.5f);
        public float inlineObjectsInterval = 3f;
        public float betweenLinesInterval = 1.5f;
        public int objectsPerOddLines = 5;
        public int objectsPerEvenLines = 6;

        public exceptionActuatorPosition[] exceptions;


        [Header("Custom Placement")]
        [SerializeField]
        private int[] actuatorsPlacementOrder = {
        18, 21, 25, 33, 26, 32, 28, 31, 27, 30, 20, 54, 24,
        38, 19, 46, 23, 53, 22, 37, 45, 8, 9, 17, 41, 56, 3,
        11, 16, 39, 48, 40, 15, 14, 4, 10, 47, 55, 50, 49, 57,
        29, 12, 2, 41, 56, 3, 11, 39, 48, 40, 15, 14, 4, 10, 47, 55 };

        // Start is called before the first frame update
        void Start()
        {
            // Using  Inspector to Call methods
        }

        private void InitializeActuatorGrid()
        {
            // Calculate the total number of lines and the maximum items per line
            int totalLines = Mathf.CeilToInt((float)(arrayOfActuators.Length + exceptions.Length) / Math.Min(objectsPerOddLines, objectsPerEvenLines));  // to get max line possible
            int maxItemsPerLine = Math.Max(objectsPerOddLines, objectsPerEvenLines);   // to get max obj in line          
            // Initialize the 2D array
            actuatorGrid = new Actuator[totalLines, maxItemsPerLine];
        }

        public void SetAllChildObjectsAndActuator()
        {
            InitializeActuatorGrid();

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

            SetAllChildObjectsAndActuator();

            if (arrayOfActuators == null || actuatorsPlacementOrder == null || actuatorsPlacementOrder.Length == 0)
            {
                Debug.LogError("Actuators array or placement order is not initialized or empty.");
                return;
            }

            Vector3 position = startPosition;
            int actuatorIndex = 0;
            int lineNumber = 0;  // Use zero-based indexing for easier array management

            while (actuatorIndex < arrayOfActuators.Length)
            {
                int objectsInLine = (lineNumber % 2 == 0) ? objectsPerOddLines : objectsPerEvenLines;

                for (int obj = 0; obj < objectsInLine; obj++)
                {
                    if (actuatorIndex >= arrayOfActuators.Length) break;

                    if (!ShouldSkipPosition(lineNumber + 1, obj + 1))
                    {
                        Actuator currentActuator = arrayOfActuators[actuatorsPlacementOrder[actuatorIndex]];
                        currentActuator.actuatorObject.transform.localPosition = position;
                        currentActuator.actuatorObject.transform.rotation = Quaternion.identity;
                        //currentActuator.matrixPosition = new Vector2Int(lineNumber, obj);

                        actuatorGrid[lineNumber, obj] = currentActuator;

                       // Debug.Log($"Placing Actuator: {currentActuator.name}, Array Index: {actuatorsPlacementOrder[actuatorIndex]}, Grid Position: {currentActuator.matrixPosition}");
                        actuatorIndex++;
                    }

                    position.z += inlineObjectsInterval; // Move to next position
                }

                // Move to the next line
                
                position.x += betweenLinesInterval;
                position.z = startPosition.z; // Reset the z position to the start of the next line
                if (lineNumber % 2 == 0) position += offsetOnSecondLine; // Apply offset for even lines
                lineNumber++;
            }
        }

        private bool ShouldSkipPosition(int lineNumber, int objectNumber)
        {
            foreach (var exception in exceptions)
            {
                if (exception.numberOfLine == lineNumber && exception.numberOfItemInRaw == objectNumber)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
