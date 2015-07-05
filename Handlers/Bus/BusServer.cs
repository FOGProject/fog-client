using System;
using SuperSocket.SocketBase;

namespace FOG.Handlers
{
    class BusServer
    {
        public AppServer Socket { get; private set; }
        private const string LogName = "Bus::Server";

        public BusServer(int port)
        {
            Socket = new AppServer();

            if (!Socket.Setup(port))
               Log.Error(LogName, "Could not start server on port " + port);
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
