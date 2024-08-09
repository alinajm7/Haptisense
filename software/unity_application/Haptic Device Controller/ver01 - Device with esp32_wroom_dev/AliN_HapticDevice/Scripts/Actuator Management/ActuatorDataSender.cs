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

        // Constructor for ActuatorDataSender class.
        public ActuatorDataSender(ActuatorsManager actuatorsManager, MicrocontrollerCommunicationManager communicationManager)
        {
            AssignClassesAutomatically();
        }

        // Use Unity's Awake to initialize
        private void Awake()
        {
            AssignClassesAutomatically();
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
                // Fetch actuator data from actuatorsManager and send it to the microcontroller.
                communicationManager.SendActuatorValueToMicrocontroller(actuatorsManager.arrayOfActuators);
            
        }  

    }
}
