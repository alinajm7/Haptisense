using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using UnityEditor;
using AliN.Microcontroller;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(MicrocontrollerCommunicationManager))]
public class MicrocontrollerCommunicationManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //Draw Header "Serial Port Configurations"

        EditorGUILayout.LabelField("Serial Port Configurations", EditorStyles.boldLabel); // Custom header

        MicrocontrollerCommunicationManager manager = (MicrocontrollerCommunicationManager)target;

        // Find available ports
        string[] portNames = SerialPort.GetPortNames();
        manager.AvailablePorts = portNames.ToList();

        // Validate SelectedPortIndex
        if (manager.SelectedPortIndex < 0 || manager.SelectedPortIndex >= manager.AvailablePorts.Count)
        {
            manager.SelectedPortIndex = -1;
        }

        // Create dropdown
        if (manager.AvailablePorts.Count > 0)
        {
            manager.SelectedPortIndex = EditorGUILayout.Popup("Select Port", manager.SelectedPortIndex, manager.AvailablePorts.ToArray());
            manager.PortName = manager.AvailablePorts[manager.SelectedPortIndex];
        }
        else
        {
            EditorGUILayout.LabelField("No ports available");
        }

        // Create dropdown for BaudRate
        int[] baudRates = new int[] { 9600, 14400, 19200, 38400, 57600, 115200, 921600 };
        List<string> baudRateStrings = baudRates.Select(b => b.ToString()).ToList();
        int defaultBaudRateIndex = Array.IndexOf(baudRates, manager.BaudRate); // Finding the index of default value

        if (defaultBaudRateIndex == -1) defaultBaudRateIndex = 0;  // Fallback if the current baud rate is not in the list

        int selectedBaudRateIndex = EditorGUILayout.Popup("BaudRate", defaultBaudRateIndex, baudRateStrings.ToArray());
        manager.BaudRate = baudRates[selectedBaudRateIndex];

        EditorGUILayout.Space();  // Adds some space

        DrawDefaultInspector();  // Draws the default inspector below your custom fields

        EditorGUILayout.Space();  // Adds some space

        //Add a button to execute some function
        if (GUILayout.Button("Re-connect"))
        {
            manager.InitializeCommunicationHandler();  // Replace `SomeFunction` with the actual function you want to call
        }
        EditorGUILayout.Space();  // Adds some space
    }
}
#endif
