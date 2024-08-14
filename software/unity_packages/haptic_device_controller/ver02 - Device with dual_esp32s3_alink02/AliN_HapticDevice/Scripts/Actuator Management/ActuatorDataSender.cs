using AliN.Microcontroller.Classes;
using System.Collections.Generic;
using UnityEngine;

namespace AliN.Microcontroller
{
    //[RequireComponent(typeof(ActuatorsManager))]
    public class ActuatorDataSender : MonoBehaviour
    {
        [SerializeField]
        private ActuatorsManager[] actuatorsManagers;

        [SerializeField]
        private MicrocontrollerCommunicationManager communicationManager;

        [Tooltip("Specifies the number of additional data transmissions to ensure all actuator updates are sent, preventing the microcontroller from missing any data during sudden movements.")]
        public int sendDataSafetyMargin = 120;
        private int sendDataSafetyCounter;

        private int[] lastLowStateDurations;
        private int[] lastHighStateDurations;

        // Maximum value for clamping
        private const int MaxClampValue = 99999;

        bool dataChanged = false;

        private void Awake()
        {
            AssignClassesAutomatically();
            sendDataSafetyCounter = sendDataSafetyMargin;

            // Initialize arrays based on the number of actuators
            if (actuatorsManagers.Length > 0 && actuatorsManagers[0].arrayOfActuators.Length > 0)
            {
                int arrayLength = actuatorsManagers[0].arrayOfActuators.Length;
                lastLowStateDurations = new int[arrayLength];
                lastHighStateDurations = new int[arrayLength];
            }
            else
            {
                Debug.LogError("No actuators found in ActuatorsManager.");
            }
        }


        void AssignClassesAutomatically()
        {
            if (actuatorsManagers == null || actuatorsManagers.Length == 0)
            {
                actuatorsManagers = FindObjectsOfType<ActuatorsManager>();

                if (actuatorsManagers == null || actuatorsManagers.Length == 0)
                {
                    Debug.LogError("No ActuatorsManager found in the scene.");
                }
            }

            if (communicationManager == null)
            {
                communicationManager = FindObjectOfType<MicrocontrollerCommunicationManager>();
                if (communicationManager == null)
                {
                    Debug.LogError("CommunicationManager not found");
                }
            }
        }

        // Method to combine all arrays from all ActuatorsManager components
        private Actuator[] CombineAndClampAllActuatorArrays()
        {
            if (actuatorsManagers == null || actuatorsManagers.Length == 0)
            {
                Debug.LogError("No ActuatorsManager components found.");
                return null;
            }

            int arrayLength = actuatorsManagers[0].arrayOfActuators.Length;
            Actuator[] combinedArray = new Actuator[arrayLength];

            // Initialize the combined array
            for (int i = 0; i < arrayLength; i++)
            {
                combinedArray[i] = new Actuator();             
            }

            // Sum the lowStateDuration and highStateDuration from all ActuatorsManagers
            for (int k = 0; k < actuatorsManagers.Length; k++)
            {
                for (int i = 0; i < arrayLength; i++)
                {
                    combinedArray[i].lowStateDuration += actuatorsManagers[k].arrayOfActuators[i].lowStateDuration;
                    combinedArray[i].highStateDuration += actuatorsManagers[k].arrayOfActuators[i].highStateDuration;
                }
            }

            // Clamp the values to the maximum allowed value
            for (int i = 0; i < arrayLength; i++)
            {
                combinedArray[i].lowStateDuration = Mathf.Clamp(combinedArray[i].lowStateDuration, 0, MaxClampValue);
                combinedArray[i].highStateDuration = Mathf.Clamp(combinedArray[i].highStateDuration, 0, MaxClampValue);
            }

            return combinedArray;
        }

        // Update method
        public void Update()
        {

            Actuator[] combinedArray = CombineAndClampAllActuatorArrays();
           

            // Check if data has changed
            for (int i = 0; i < combinedArray.Length; i++)
            {
                // Check if the values have changed
                if (combinedArray[i].highStateDuration != lastHighStateDurations[i] || combinedArray[i].lowStateDuration != lastLowStateDurations[i])
                {
                    // Update the last known values
                    lastLowStateDurations[i] = combinedArray[i].lowStateDuration;
                    lastHighStateDurations[i] = combinedArray[i].highStateDuration;
                    dataChanged = true;
                }
            }

            if (dataChanged)
            {
                communicationManager.SendActuatorValueToMicrocontroller(combinedArray);

                if (sendDataSafetyCounter > 0)
                {
                    sendDataSafetyCounter--;
                }
                else
                {
                    sendDataSafetyCounter = sendDataSafetyMargin;
                    dataChanged = false;
                }
            }
        }
    }

}
