using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO.Ports;
using System.Collections.Generic;
using System.Linq;

namespace AliN.Microcontroller
{
    public class MicrocontrollerCommunicationSettingUIHandler : MonoBehaviour
    {
        public MicrocontrollerCommunicationManager communicationManager;

        // UI elements
        public TMP_Dropdown portDropdown;
        public TMP_Dropdown baudRateDropdown;
        public Button refreshButton; // Button for refreshing the connection
        public Image connectionStatusImage; // Image to indicate connection status

        public Color connectedColor = Color.green;
        public Color disconnectedColor = Color.red;

        private void OnEnable()
        {
            RefreshMicrocontrollerConnection();
        }
        void Start()
        {
            
            InitializePortDropdown();
            InitializeBaudRateDropdown();
            InitializeRefreshButton();
            RefreshMicrocontrollerConnection();
        }

        void InitializePortDropdown()
        {
            var ports = SerialPort.GetPortNames();
            portDropdown.ClearOptions();
            portDropdown.AddOptions(ports.ToList());

            int currentPortIndex = ports.ToList().IndexOf(communicationManager.PortName);
            if (currentPortIndex != -1)
            {
                portDropdown.value = currentPortIndex;
            }

            portDropdown.onValueChanged.AddListener(delegate
            {
                UpdatePortSelection(portDropdown.options[portDropdown.value].text);
                RefreshMicrocontrollerConnection(); // Attempt to connect and update status
            });
        }

        void InitializeBaudRateDropdown()
        {
            var baudRates = new List<int> { 9600, 19200, 38400, 57600, 115200, 921600 };
            baudRateDropdown.ClearOptions();
            baudRateDropdown.AddOptions(baudRates.Select(b => b.ToString()).ToList());

            int currentBaudRateIndex = baudRates.IndexOf(communicationManager.BaudRate);
            if (currentBaudRateIndex != -1)
            {
                baudRateDropdown.value = currentBaudRateIndex;
            }

            baudRateDropdown.onValueChanged.AddListener(delegate
            {
                UpdateBaudRateSelection(baudRates[baudRateDropdown.value]);
                RefreshMicrocontrollerConnection(); // Attempt to connect and update status
            });
        }

        void InitializeRefreshButton()
        {
            refreshButton.onClick.AddListener(delegate
            {
                RefreshMicrocontrollerConnection();
            });
        }

        public void UpdatePortSelection(string selectedPort)
        {
            communicationManager.UpdateSerialSettings(selectedPort, communicationManager.BaudRate);
        }

        public void UpdateBaudRateSelection(int selectedBaudRate)
        {
            communicationManager.UpdateSerialSettings(communicationManager.PortName, selectedBaudRate);
        }

        public void RefreshMicrocontrollerConnection()
        {
           // communicationManager.RefreshConnection();
            UpdateConnectionStatusImage(communicationManager.isConnected); // Update connection status
        }

    

        private void UpdateConnectionStatusImage(bool isConnected)
        {
            connectionStatusImage.color = isConnected ? connectedColor : disconnectedColor;
        }

        // Add methods to update WiFi settings and other functionalities if necessary
    }
}
