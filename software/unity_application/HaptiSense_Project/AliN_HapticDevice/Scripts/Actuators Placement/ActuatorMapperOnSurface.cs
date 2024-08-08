using UnityEngine;
using AliN.Microcontroller.Classes;


namespace AliN.Microcontroller
{
    [RequireComponent(typeof(ActuatorLayout))]
    [ExecuteInEditMode]
    public class ActuatorMapperOnSurface : MonoBehaviour
    {
        public enum MappingType
        {
            SimpleSurfaceMapping,
            ReferenceBasedSurfaceMapping,
            DistanceMaintainedSurfaceMapping
        }


        public enum AxisDirection
        {
            Positive_X,
            Negative_X,
            Positive_Y,
            Negative_Y,
            Positive_Z,
            Negative_Z
        }
        [Header("Mapping Surface Object")]
        public GameObject MappingSourceObject;

        [Header("Mapping Parameters")]
        public MappingType TypeOfMapping = MappingType.DistanceMaintainedSurfaceMapping;
        public AxisDirection axisDirection = AxisDirection.Negative_Y;
        [HideInInspector]
        public bool priorityIsRowDistance = true;

        [HideInInspector]
        [Header("Reference Object")]
        // Assuming the reference object is the first in the grid [0,0]
        public int referenceObjectRow = 0;
        [HideInInspector]
        public int referenceObjectCol = 0;

        [Header("Drawing Settings")]

        public Color debugLineColor = Color.green;
        public float debugLineDuration = 2.0f;
        private bool actuatorGridInitialized = false;

        private Actuator[,] actuatorGrid;
        private Vector3[,] initialRelativePositions; // To store initial relative positions

        

        void Start()
        {
            // Using Inspector/UI-editor to run this class
        }

        public void ResetActuators()
        {
            ActuatorLayout actuatorLayout = GetComponent<ActuatorLayout>();
            if (actuatorLayout != null)
            {
                actuatorLayout.ArrangeInLayout();
                actuatorGridInitialized = false;
            }
        }

        public void PlaceActuatorsOnTheSurface()
        {
            if (!actuatorGridInitialized) InitializeActuatorGrid();
            
            switch (TypeOfMapping)
            {
                case MappingType.SimpleSurfaceMapping:
                    MapObjectsToSurface();
                    break;
                case MappingType.ReferenceBasedSurfaceMapping:
                    PlaceAllObjectsOnTheSurfaceAccordingToTheReferenceObject();
                    break;
                case MappingType.DistanceMaintainedSurfaceMapping:
                    PlaceAllActuatorsOnTheSurfaceByRemainingTheirDistance();
                    break;
               
            }


        }

        private void InitializeActuatorGrid()
        {           
            ActuatorLayout actuatorLayout = GetComponent<ActuatorLayout>();
            if (actuatorLayout != null)
            {
                actuatorGrid = actuatorLayout.actuatorGrid;
                if (actuatorGrid == null || actuatorGrid.GetLength(0) == 0 || actuatorGrid.GetLength(1) == 0)
                {
                    Debug.LogError("Actuator grid is not initialized or empty.");
                    return;
                }

                // Initialize the initialRelativePositions array with the same dimensions as actuatorGrid
                initialRelativePositions = new Vector3[actuatorGrid.GetLength(0), actuatorGrid.GetLength(1)];

                // Assume the first actuator is at [0,0], and calculate relative positions from this actuator
                Vector3 firstActuatorPosition = actuatorGrid[0, 0]?.actuatorObject.transform.position ?? Vector3.zero;

                for (int i = 0; i < actuatorGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < actuatorGrid.GetLength(1); j++)
                    {
                        if (actuatorGrid[i, j] != null)
                        {
                            initialRelativePositions[i, j] = actuatorGrid[i, j].actuatorObject.transform.position - firstActuatorPosition;
                        }
                        else
                        {
                            initialRelativePositions[i, j] = Vector3.zero; // Assign zero if no actuator is present
                        }
                    }
                }
                actuatorGridInitialized = true;
            }
            else
            {
                Debug.LogError("Failed to find ActuatorLayout component.");
                actuatorGridInitialized = false;
            }
        }


        private void MapObjectsToSurface()
        {


            if (actuatorGrid == null)
            {
                Debug.LogError("Actuator grid is not initialized.");
                return;
            }

            for (int i = 0; i < actuatorGrid.GetLength(0); i++)
            {
                for (int j = 0; j < actuatorGrid.GetLength(1); j++)
                {
                    Actuator actuator = actuatorGrid[i, j];
                    if (actuator != null)
                    {
                        MapActuatorToSurface(actuator);
                    }
                }
            }

        }

        public void DrawLineFromObjectsToSurface()
        {
            if (!actuatorGridInitialized) InitializeActuatorGrid();

            if (actuatorGrid == null)
            {
                Debug.LogError("Actuator grid is not initialized.");
                return;
            }

            // Loop through each actuator in the 2D grid
            for (int i = 0; i < actuatorGrid.GetLength(0); i++)
            {
                for (int j = 0; j < actuatorGrid.GetLength(1); j++)
                {
                    Actuator child = actuatorGrid[i, j];
                    if (child != null && child.actuatorObject != null)
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
                    else
                    {
                       // Debug.LogError($"Actuator at grid position [{i},{j}] is null or missing.");
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

        private void MapActuatorToSurface(Actuator actuator)
        {
            Vector3 rayStart = actuator.actuatorObject.transform.position + new Vector3(0, 0.2f, 0);
            RaycastHit hit;

            if (Physics.Raycast(rayStart, GetAxisFromEnum(axisDirection), out hit))
            {
                if (hit.collider.gameObject == MappingSourceObject)
                {
                    actuator.actuatorObject.transform.position = hit.point;
                    actuator.actuatorObject.transform.up = hit.normal;

                    Debug.DrawLine(rayStart, hit.point, debugLineColor, debugLineDuration);
                }
                else
                {
                    Debug.DrawLine(rayStart, rayStart + GetAxisFromEnum(axisDirection) * 5f, Color.red, debugLineDuration);
                }
            }
            else
            {
                Debug.DrawLine(rayStart, rayStart + GetAxisFromEnum(axisDirection) * 5f, Color.red, debugLineDuration);
            }
        }


        private void PlaceAllActuatorsOnTheSurfaceByRemainingTheirDistance()
        {
            // Validate the actuator grid initialization
            if (actuatorGrid == null || actuatorGrid.GetLength(0) == 0 || actuatorGrid.GetLength(1) == 0)
            {
                Debug.LogError("Actuator grid is not properly initialized.");
                return;
            }

            // Map the first actuator [0,0] to the surface
            MapActuatorToSurface(actuatorGrid[0, 0]);

            // Map subsequent actuators in the first column
            for (int i = 1; i < actuatorGrid.GetLength(0); i++)
            {
                if (actuatorGrid[i, 0] != null)
                {
                    PlaceActuatorRelativeToReference(actuatorGrid[i, 0], actuatorGrid[i - 1, 0],
                        initialRelativePositions[i, 0] - initialRelativePositions[i - 1, 0]);
                }
            }

            // Map subsequent actuators in the first row
            for (int j = 1; j < actuatorGrid.GetLength(1); j++)
            {
                if (actuatorGrid[0, j] != null)
                {
                    PlaceActuatorRelativeToReference(actuatorGrid[0, j], actuatorGrid[0, j - 1],
                        initialRelativePositions[0, j] - initialRelativePositions[0, j - 1]);
                }
            }

            // Mapping other actuators depending on rowDistancePriority
            if (priorityIsRowDistance)
            {
                for (int k = 1; k < actuatorGrid.GetLength(0); k++)
                {
                    for (int i = 1; i < actuatorGrid.GetLength(1); i++)
                    {
                        if (actuatorGrid[k, i] != null)
                        {
                            Actuator referenceActuator = actuatorGrid[k - 1, i] ?? actuatorGrid[k, i - 1];
                            Vector3 relativeDirection = actuatorGrid[k - 1, i] != null ?
                                initialRelativePositions[k, i] - initialRelativePositions[k - 1, i] :
                                initialRelativePositions[k, i] - initialRelativePositions[k, i - 1];

                            PlaceActuatorRelativeToReference(actuatorGrid[k, i], referenceActuator, relativeDirection);
                        }
                    }
                }
            }
            else
            {
                for (int k = 1; k < actuatorGrid.GetLength(1); k++)
                {
                    for (int i = 1; i < actuatorGrid.GetLength(0); i++)
                    {
                        if (actuatorGrid[i, k] != null)
                        {
                            PlaceActuatorRelativeToReference(actuatorGrid[i, k], actuatorGrid[i, k - 1],
                                initialRelativePositions[i, k] - initialRelativePositions[i, k - 1]);
                        }
                    }
                }
            }
        }


        private void PlaceActuatorRelativeToReference(Actuator currentActuator, Actuator referenceActuator, Vector3 relativePosition)
        {
            if (currentActuator == null || referenceActuator == null)
            {
                Debug.LogError("One of the actuators is not initialized.");
                return;
            }

            Vector3 basePosition = referenceActuator.actuatorObject.transform.position;
            Vector3 baseNormal = referenceActuator.actuatorObject.transform.up;

            // Calculate the start point above the reference actuator along the normal
            Vector3 startPoint = basePosition + baseNormal * 10.0f;

            // Rotate the direction to align with the surface normal of the reference actuator
            Vector3 raycastDirection = Quaternion.FromToRotation(Vector3.up, baseNormal) * relativePosition.normalized;

            //// For debugging: Draw a sphere at the start point to visualize where scanning starts
            //Debug.DrawRay(basePosition, baseNormal * 10.0f, Color.red, debugLineDuration);  // Draws a line representing the normal
            //Debug.DrawLine(basePosition, startPoint, Color.green, debugLineDuration);  // Draws a line to the starting scan point
            //Debug.DrawLine(startPoint,  raycastDirection * 30.0f, Color.blue, debugLineDuration);  // Shows the scan direction


            float targetDistance = relativePosition.magnitude;
            float scanStep = 0.01f; // Step for adjusting raycast start point along the direction
            float maxScanDistance = 200.0f; // Maximum distance to scan along the surface

            RaycastHit hit;
            bool isPlacedCorrectly = false;

            // Start scanning from the startPoint in the calculated direction
            for (float d = scanStep; d <= maxScanDistance; d += scanStep)
            {
                Vector3 scanPoint = startPoint + raycastDirection * d;
                if (Physics.Raycast(scanPoint, -baseNormal, out hit))
                {
                    float surfaceDistance = (hit.point - basePosition).magnitude;
                    if (Mathf.Abs(surfaceDistance - targetDistance) < 0.1) // Acceptable accuracy
                    {
                        // Set position to hit point
                        currentActuator.actuatorObject.transform.position = hit.point;
                        // Align actuator normal to the hit normal
                        currentActuator.actuatorObject.transform.up = hit.normal;
                        isPlacedCorrectly = true;
                        break;
                    }
                }
            }

            if (!isPlacedCorrectly)
            {
                Debug.LogError("Failed to place actuator correctly on the surface.");
            }
        }


        private void PlaceAllObjectsOnTheSurfaceAccordingToTheReferenceObject()
        {
            if (actuatorGrid == null || actuatorGrid.GetLength(0) == 0 || actuatorGrid.GetLength(1) == 0)
            {
                Debug.LogError("Actuator grid is not initialized or empty.");
                return;
            }

            if (initialRelativePositions == null)
            {
                Debug.LogError("Initial relative positions array is not set.");
                return;
            }         

            RaycastHit hit;
            Vector3 raycastOrigin = actuatorGrid[referenceObjectRow, referenceObjectCol].transform.position + GetAxisFromEnum(axisDirection) * 0.1f;

            if (Physics.Raycast(raycastOrigin, GetAxisFromEnum(axisDirection), out hit, Mathf.Infinity))
            {
                if (hit.collider.gameObject == MappingSourceObject)
                {
                    actuatorGrid[referenceObjectRow, referenceObjectCol].actuatorObject.transform.position = hit.point;
                    actuatorGrid[referenceObjectRow, referenceObjectCol].actuatorObject.transform.up = hit.normal;
                }
                else
                {
                    Debug.LogError($"Raycast did not hit the expected MappingSourceObject, instead hit: {hit.collider.gameObject.name}");
                    return;
                }
            }
            else
            {
                Debug.LogError("Raycast from referenceObject did not hit any object.");
                return;
            }

            Vector3 firstActuatorPosition = actuatorGrid[referenceObjectRow, referenceObjectCol].actuatorObject.transform.position;
            Vector3 firstActuatorNormal = hit.normal;

            for (int j = 0; j < actuatorGrid.GetLength(1); j++)
            {
                for (int i = 0; i < actuatorGrid.GetLength(0); i++)
                {
                    if (actuatorGrid[i, j] == null)
                    {
                        //Debug.LogError($"Actuator at [{i},{j}] is null.");
                        continue;
                    }

                    if (i == referenceObjectRow && j == referenceObjectCol) continue; // Skip the reference actuator

                    float pathDistance = (initialRelativePositions[i, j] - initialRelativePositions[referenceObjectRow, referenceObjectCol]).magnitude;
                    Vector3 direction = (initialRelativePositions[i, j] - initialRelativePositions[referenceObjectRow, referenceObjectCol]).normalized;

                    Vector3 currentPosition = firstActuatorPosition;
                    Vector3 currentNormal = firstActuatorNormal;
                    int numSteps = (int)Mathf.Ceil(pathDistance / 0.01f);
                    float stepSize = pathDistance / numSteps;

                    for (int step = 0; step < numSteps; step++)
                    {
                        Vector3 stepDirection = Vector3.ProjectOnPlane(direction, currentNormal);
                        Vector3 nextStepPosition = currentPosition + stepDirection * stepSize;

                        if (Physics.Raycast(nextStepPosition + currentNormal * 0.01f, -currentNormal, out hit, stepSize + 0.01f))
                        {
                            currentPosition = hit.point;
                            currentNormal = hit.normal;
                        }
                        else
                        {
                            Debug.LogError($"Raycast failed to find surface at step {step} for actuator [{i},{j}].");
                            break;
                        }
                    }

                   // Debug.Log($"Setting position for actuator [{i},{j}].");
                    actuatorGrid[i, j].actuatorObject.transform.position = currentPosition;
                    actuatorGrid[i, j].actuatorObject.transform.up = currentNormal;
                }
            }
        }


    }


}
