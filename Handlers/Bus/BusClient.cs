using System;
using WebSocket4Net;

namespace FOG.Handlers
{
    class BusClient
    {
        public WebSocket Socket { get; private set; }
        private const string LogName = "Bus::Client";

        public BusClient(int port)
        {
            Socket = new WebSocket("ws://localhost:" + port + "/");
        }

        public void Start()
        {
            Socket.Open();
        }

        public void Stop()
        {
            Socket.Close();
            Socket.Dispose();
        }

        public void Send(string message)
        {
            try
            {
                Socket.Send(message);
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not send message");
                Log.Error(LogName, ex);
            }
        }
    }
}
