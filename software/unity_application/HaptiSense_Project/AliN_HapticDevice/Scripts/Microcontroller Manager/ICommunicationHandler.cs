using System;

namespace AliN.Microcontroller
{
    public interface ICommunicationHandler : IDisposable
    {
        void Connect();
        void SendStructuredData(string stringdata);
        void CleanBuffer();
        bool IsConnected { get; }
    }
}
