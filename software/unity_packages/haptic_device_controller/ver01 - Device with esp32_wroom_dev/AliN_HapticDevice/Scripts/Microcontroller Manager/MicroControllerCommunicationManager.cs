using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using AliN.Microcontroller.ESP32;
using AliN.Microcontroller.Classes;
using System.Text;
using System.Collections;

namespace AliN.Microcontroller
{
    // Define available log levels
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    // Communication manager for managing microcontroller interactions
    public class MicrocontrollerCommunicationManager : Singleton<MicrocontrollerCommunicationManager>
    {
        private readonly object _lock = new object(); // For thread safety
        private ICommunicationHandler _communicationHandler;

        // UnityEdit (end of this script) I added method to create a dropbox to select PortName = "COM5"
        // Fields for Serial connection configuration 
        //[HideInInspector]
        public int BaudRate = 921600;


        [HideInInspector]
        public List<string> AvailablePorts;
        [HideInInspector]
        public int SelectedPortIndex = 0; // This will hold the index of the selected port in the dropdown
        //[HideInInspector]
        public string PortName = "COM5";

        // Fields for WiFi configuration
        [Header("---------- Connect by WiFi instead of Serial Port ----------")]

        public bool connectByWiFi = false;

        [Header("---------- WIFI Configurations ----------")]
        // Router - WIFI info
        [SerializeField]
        private string ServerIP = "192.168.14.1";
        [SerializeField]
        private int ServerPort = 8888; // Choose any available port number

        // Fields for Actuator data remapping
        [Header("---------- Remapping Of Actuator data ----------")]
        [Tooltip("The calculated value in the app is between 0 to 1 but microcontroller need value between 0 to 4096")]
        [SerializeField]
        private float minActuatorValue = 0;
        [SerializeField]
        private float maxActuatorValue = 1;
        [SerializeField]
        private float remappedMinValue = 0;
        [SerializeField]
        private float remappedMaxValue = 4000;

        [Header("---------- Sending Data Setting ----------")]
        [SerializeField]
        private int sendingDataInterval = 20;

        [Header("---------- Microcontroller behaviour Setting ---------- ")]
        public bool activeInterval = false;
        public bool activeRelativeInterval = false;
        [Tooltip("milliSecond")]
        public int activeDuration = 100;
        [Tooltip("milliSecond")]
        public int intervalDuration = 300;
        [Tooltip("Percentage")]
        public int relativeIntervalValue = 10;
        [Tooltip("0000-4062")]
        public int intervalValueFix = 2000;

        [Header("---------- Re-connect Button ---------- ")]
        // Coroutine state variable
        // Serves as a flag to manage Coroutine execution, ensuring data is sent with a delay to prevent overloading the microcontroller.
        private int isCoroutineActive  = 0;

        public bool isConnected { get { return _communicationHandler.IsConnected; } }

        // Initialization
        void Start()
        {
            InitializeCommunicationHandler();
        }
        public void RestartMicrocontrollerCommunicationManager()
        {
            // Close the open port and clean up any resources
            CloseTheOpenPort();
            // Initialize the communication handler with the selected port
            InitializeCommunicationHandler();

            // Connect to the selected port
            _communicationHandler.Connect();

            //// Destroy the existing singleton instance
            //Destroy(this.gameObject);

            //// Create a new instance of the MicrocontrollerCommunicationManager
            //Instantiate(this);
        }
        public void DestroyMicrocontrollerCommunicationManager()
        {
            // Close the open port and clean up any resources
            CloseTheOpenPort();

            // Destroy the existing singleton instance
            Destroy(this.gameObject);

        }

        // Initialize the correct communication channel
        public void InitializeCommunicationHandler()
        {
            //Close anything if it is open
            if (_communicationHandler != null) _communicationHandler.Dispose();
            // Connect to desire port
            if (connectByWiFi)
            {
                _communicationHandler = new WiFiHandler(ServerIP, ServerPort);
            }
            else
            {
                _communicationHandler = new SerialPortHandler(PortName, BaudRate);
            }
            _communicationHandler.Connect();
        }

        public void UpdateSerialSettings(string newPortName, int newBaudRate)
        {
            PortName = newPortName;
            BaudRate = newBaudRate;
            RefreshConnection();
        }

        public void UpdateWiFiSettings(string newServerIP, int newServerPort)
        {
            ServerIP = newServerIP;
            ServerPort = newServerPort;
            RefreshConnection();
        }

        public void RefreshConnection()
        {
            // Stop coroutine if it is active. If I don't do this and change the serialport during running it will not be affective.
            if (Interlocked.CompareExchange(ref isCoroutineActive, 0, 1) == 1)
            {
                StopAllCoroutines(); // This stops all coroutines. If you have multiple coroutines, you may need a more targeted approach.
            }
            //// Re Connect it again
            //InitializeCommunicationHandler();
            RestartMicrocontrollerCommunicationManager();
        }

        // Function to remap actuator values
        private int RemapValue(float value)
        {
            float remapped = (value - minActuatorValue) / (maxActuatorValue - minActuatorValue) * (remappedMaxValue - remappedMinValue) + remappedMinValue;
            return Mathf.RoundToInt(remapped);
        }

        // Logic for sending actuator values
        public void SendActuatorValueToMicrocontroller(Actuator[] actuatorArray)
        {
            if (Interlocked.CompareExchange(ref isCoroutineActive , 1, 0) == 0)
            {
                string dataString = StructureDataForSending(actuatorArray);
                StartCoroutine(SendDataToMicroControllerWithDelay(dataString));
            }
        }

        // Constructing the Structure of data that I designed for Microcontroller
        // Getting 206 number in a 206 byte length string. first 14 number are setting info and then every 4 number is actuator value. from actuator 0 to actuator 47
        private string StructureDataForSending(Actuator[] actuatorArray)
        {
            StringBuilder sb = new StringBuilder(14 + (48*4)); // Pre-allocate enough space for 48 actuators * 4 characters each

            // Add control characters
            sb.Append(activeInterval ? "1" : "0");
            sb.Append(activeRelativeInterval ? "1" : "0");
            sb.AppendFormat("{0:D3}", activeDuration);
            sb.AppendFormat("{0:D3}", intervalDuration);
            sb.AppendFormat("{0:D2}", relativeIntervalValue);
            sb.AppendFormat("{0:D4}", intervalValueFix);

            for (int i = 0; i < 48; i++)
            {
                float newValue = RemapValue(actuatorArray[i].actuatorValue);

                sb.AppendFormat("{0:D4}", (int)newValue); // assuming that actuatorValue is a float and you want to cast it to an integer.
            }

            string dataString = sb.ToString();
            return dataString;
        }

        public void ResetAllActuators()
        {
            // create a new array of Actuators and set all the actuator.values to zero and send to the micro controller
            Debug.Log("ResetAllActuators  Called");
                Actuator[] zeroedActuators = new Actuator[48];
                for (int i = 0; i < 48; i++)
                {
                zeroedActuators[i] = new Actuator(); //zeroedActuators[i].actuatorValue = 0 in Actuator class
                }

            string sendingZeroString = StructureDataForSending(zeroedActuators);
            _communicationHandler.SendStructuredData(sendingZeroString);
        }

        // Coroutine for delayed data sending and prevent over loading data to microcontroller
        IEnumerator SendDataToMicroControllerWithDelay(string sendingData)
        {
            yield return new WaitForSeconds(sendingDataInterval / 1000f); // Convert milliseconds to seconds
            _communicationHandler.SendStructuredData(sendingData);
            Interlocked.Exchange(ref isCoroutineActive , 0); // Set isWaiting back to 0
        }

        public void CleanBuffer()
        {
            // Just clean Buffer
            _communicationHandler.CleanBuffer();
        }


        private void OnApplicationQuit()
        {
            CloseTheOpenPort();
        }

        public void CloseTheOpenPort()
        {
            if (_communicationHandler != null)
            {
                if (_communicationHandler.IsConnected)
                {
                    // Clean Buffers and close open ports
                    ResetAllActuators();
                    CleanBuffer();
                }
                _communicationHandler.Dispose();
            }
        }

        public void LogMessage(string message, LogLevel level = LogLevel.Info)
        {
            lock (_lock) // Thread safety
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logMessage = $"[{timestamp}] [{level}] {message}";

                switch (level)
                {
                    case LogLevel.Info:
                        Debug.Log(logMessage);
                        break;
                    case LogLevel.Warning:
                        Debug.LogWarning(logMessage);
                        break;
                    case LogLevel.Error:
                        Debug.LogError(logMessage);
                        break;
                }
            }
        }
    }
}
