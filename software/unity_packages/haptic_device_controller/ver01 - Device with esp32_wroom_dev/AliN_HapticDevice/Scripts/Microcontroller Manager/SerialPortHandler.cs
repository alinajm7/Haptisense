using System;
using System.IO;
using System.IO.Ports;
using System.Text;


namespace AliN.Microcontroller.ESP32
{

    public class SerialPortHandler : ICommunicationHandler
    {
        // Private variables
        private string _portName = "COM5";
        private int _baudRate = 921600;
        private SerialPort _serialPort;
        private bool _isConnected;

        // Property to check if connected
        public bool IsConnected => _isConnected;

        // Constructor to initialize Serial Port settings
        public SerialPortHandler(string portName, int boudRate)
        {
            _portName = portName;
            _baudRate = boudRate;

            //_serialPort = new SerialPort(this._portName, _baudRate);
        }

        // Method to initiate the connection
        public void Connect()
        {
            _serialPort = new SerialPort(this._portName, _baudRate);
            try
            {
                _serialPort.Open();
                _isConnected = true;
                MicrocontrollerCommunicationManager.Instance.LogMessage("Connected to Serial Port: " + _serialPort.PortName);
            }
            catch (IOException ioException)
            {
                // Logging IO exceptions
                MicrocontrollerCommunicationManager.Instance.LogMessage($"IO Error: {ioException.Message}", LogLevel.Error);
                _isConnected = false;
            }
            catch (Exception e)
            {
                // Logging other exceptions
                MicrocontrollerCommunicationManager.Instance.LogMessage($"An error occurred: {e.Message}", LogLevel.Error);
                _isConnected = false;
            }
        }

        // Method to send structured data through Serial Port
        public void SendStructuredData(string stringdata)
        {
            //_serialPort.Write(stringdata);
            byte[] byteArray = Encoding.ASCII.GetBytes(stringdata);

            // Ensure the data length is as expected before sending
            if (byteArray.Length == 206) 
            {
                _serialPort.Write(byteArray, 0, byteArray.Length);
            }
            else
            {
                // Log a warning if data length is incorrect
                MicrocontrollerCommunicationManager.Instance.LogMessage("Data length mismatch. Not sent.", LogLevel.Warning);
            }
        }


        // Method to clear input and output buffers
        public void CleanBuffer()
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
            }
            MicrocontrollerCommunicationManager.Instance.LogMessage("Refreshed the Serial Port Buffer");
        }

        // Method to dispose of unmanaged resources
        public void Dispose()
        {
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.DiscardInBuffer();
                    _serialPort.DiscardOutBuffer();
                    _serialPort.Close();
                }
                _serialPort.Dispose();
            }
            _isConnected = false;
            System.Threading.Thread.Sleep(100);
        }
    }
}
