using System;
using System.Linq;
using SuperSocket.SocketEngine;
using SuperWebSocket;

namespace FOG.Handlers
{
    class BusServer
    {
        public WebSocketServer Socket { get; private set; }
        private const string LogName = "Bus::Server";

        public BusServer()
        {
            var bootstrap = BootstrapFactory.CreateBootstrap();
            bootstrap.Initialize();
            bootstrap.Start();
            Socket = bootstrap.AppServers.FirstOrDefault() as WebSocketServer;
        }

        public bool Start()
        {
            return Socket.Start();
        }

        public void Stop()
        {
            Socket.Stop();
            Socket.Dispose();
        }

        public void Send(string message)
        {
            foreach (var session in Socket.GetAllSessions())
                try
                {
                    session.Send(message);
                }
                catch (Exception ex)
                {
                    Log.Error(LogName, "Could not send message");
                    Log.Error(LogName, ex);
                }
        }
    }
}
