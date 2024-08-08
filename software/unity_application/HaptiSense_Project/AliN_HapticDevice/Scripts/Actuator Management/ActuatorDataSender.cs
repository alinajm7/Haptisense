using AliN.Microcontroller.Classes;
using UnityEngine;


namespace AliN.Microcontroller
{
    [RequireComponent(typeof(ActuatorsManager))]
    // The ActuatorDataSender class is responsible for coordinating the sending of actuator data to the microcontroller.
    public class ActuatorDataSender : MonoBehaviour
    {
        // Reference to the ActuatorsManager to fetch the current actuator settings.
        [SerializeField]
        private ActuatorsManager actuatorsManager;

        // Reference to the MicrocontrollerCommunicationManager to handle the communication protocols.

        [SerializeField]
        private MicrocontrollerCommunicationManager communicationManager;

        [Tooltip("Specifies the number of additional data transmissions to ensure all actuator updates are sent, preventing the microcontroller from missing any data during sudden movements.")]
        public int sendDataSafetyMargin = 120;
        private int sendDataSafetyCounter;

        // Constructor for ActuatorDataSender class.
        public ActuatorDataSender(ActuatorsManager actuatorsManager, MicrocontrollerCommunicationManager communicationManager)
        {
            AssignClassesAutomatically();
        }

        // Use Unity's Awake to initialize
        private void Awake()
        {
            AssignClassesAutomatically();
            sendDataSafetyCounter = sendDataSafetyMargin;
        }

        void AssignClassesAutomatically()
        {
            if (actuatorsManager == null)
            {
                actuatorsManager = GetComponent<ActuatorsManager>();
                if (actuatorsManager == null)
                {
                    Debug.LogError("ActuatorsManager not found");
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

        // Update
        public void Update()
        {
            if (Actuator.actuatorValueChanged)
            {
                // Fetch actuator data from actuatorsManager and send it to the microcontroller.
                communicationManager.SendActuatorValueToMicrocontroller(actuatorsManager.arrayOfActuators);

                if (sendDataSafetyCounter > 0)
                {
                    sendDataSafetyCounter--;

                }
                else
                {
                    Actuator.actuatorValueChanged = false;
                    sendDataSafetyCounter = sendDataSafetyMargin;
                }
                
            }
            
        }  

    }
}
