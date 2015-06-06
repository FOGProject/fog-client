using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Reflection;

namespace FOG.Handlers
{
    public static class Bus
    {

        public enum Channel
        {

        }

        private static readonly Dictionary<Channel, LinkedList<Action<string>>> Registrar =
            new Dictionary<Channel, LinkedList<Action<string>>>();

        private const string LogName = "Bus";
        private static readonly bool Initialized = InitializePipe();
        private static PipeServer _server;
        private static PipeClient _client;

        private static bool InitializePipe()
        {
            try
            {
                _server = new PipeServer("fog-bus");
                _server.MessageReceived += pipe_RecieveMessage;
                _server.Start();
                Log.Entry(LogName, "Became bus server");

                return true;
            } catch (Exception) {}

            try
            {
                _client = new PipeClient("fog-bus");
                _client.MessageReceived += pipe_RecieveMessage;
                _client.Connect();
                Log.Entry(LogName, "Became bus client");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not enter named pipe");
                Log.Error(LogName, ex);
            }

            return false;
        }

        private static void SendMessage(string msg)
        {
            if (!Initialized) return;

            if(_server != null && _server.IsRunning())
                _server.SendMessage(msg);

            if (_client != null && _client.IsConnected())
                _client.SendMessage(msg);
        }

        public static void Emit(Channel channel, string data, bool global = false)
        {
            if (global)
            {
                SendMessage(channel + "//" + data);

                // If this bus instance is a client, wait for the event to be bounced-back before processing
                if(_client != null)
                    return;
            }

            Log.Entry(LogName, "Emmiting message on channel: " + channel);

            if (!Registrar.ContainsKey(channel)) return;

            foreach(var action in Registrar[channel])
                action.Invoke(data);
        }

        public static void Register(Channel channel, Action<string> action)
        {
            Log.Entry(LogName, string.Format("Registering {0} in channel {1}", action.Method.Name, channel));

            if (!Registrar.ContainsKey(channel))
                Registrar.Add(channel, new LinkedList<Action<string>>());
            if (Registrar[channel].Contains(action)) return;

            Registrar[channel].AddLast(action);
        }

        public static void UnRegister(Channel channel, Action<string> action)
        {
            Log.Entry(LogName, string.Format("UnRegistering {0} in channel {1}", action.Method.Name, channel));

            if (!Registrar.ContainsKey(channel)) return;
            Registrar[channel].Remove(action);
        }

        private static void pipe_RecieveMessage(Client client, string message)
        {
            EmitMessageFromPipe(message);
            SendMessage(message);
        }

        private static void pipe_RecieveMessage(string message)
        {
            EmitMessageFromPipe(message);
        }

        private static void EmitMessageFromPipe(string message)
        {
            try
            {
                var rawChannel = message.Substring(0, message.IndexOf("//"));
                var channel = (Channel) Enum.Parse(typeof(Channel), rawChannel);
                var data = message.Remove(rawChannel.Length);
                Emit(channel, data);
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not parse message from pipe");
                Log.Error(LogName, ex);

            }
        }

    }
}