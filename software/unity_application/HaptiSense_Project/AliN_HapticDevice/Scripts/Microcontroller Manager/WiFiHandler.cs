using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AliN.Microcontroller.ESP32
{
    public class WiFiHandler : ICommunicationHandler
    {
        // Private fields
        private UdpClient _udpClient;
        private IPEndPoint _endPoint;
        private readonly string _ipAddress = "192.168.14.1";
        private readonly int _port = 8080;
        private bool _isConnected;

        // Public property to check connection status
        public bool IsConnected => _isConnected;

        // Constructor to initialize properties
        public WiFiHandler(string ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
            _udpClient = new UdpClient();
            _endPoint = new IPEndPoint(IPAddress.Parse(_ipAddress), _port);
        }

        // Method to establish a connection
        public void Connect()
        {
            try
            {
                // UDP is connectionless, but I check the network
                _udpClient.Connect(_endPoint);
                _isConnected = true;
            }
            catch (Exception e)
            {
                // Logging the error message
                MicrocontrollerCommunicationManager.Instance.LogMessage($"An error occurred: {e.Message}", LogLevel.Error);
                _isConnected = false;
            }
        }

        // Method to send structured data
        public void SendStructuredData(string stringdata)
        {
            
            //_serialPort.Write(stringdata); // this is working also but  the following is better because of performance

            byte[] dataBytes = Encoding.ASCII.GetBytes(stringdata);
            // Checking the length of data before sending
            if (dataBytes.Length == 206) 
            {              
                _udpClient.Send(dataBytes, dataBytes.Length, _endPoint);
            }
            else
            {
                // Logging a warning message
                MicrocontrollerCommunicationManager.Instance.LogMessage("Data length mismatch. Not sent.", LogLevel.Warning);
            }
        }


        public void CleanBuffer()
        {
            // Not applicable for UDP clients

        }

        public void Dispose()
        {
            // Closing the UDP client
            _udpClient.Close();
            // Updating the connection status
            _isConnected = false;
        }
    }
}
