using System;
using System.Collections.Generic;

namespace FOG.Handlers
{
    public static class Bus
    {
        public enum Channel
        {
            Foo,
            Bar
        }

        private static readonly Dictionary<Channel, LinkedList<Action<string>>> Registrar =
            new Dictionary<Channel, LinkedList<Action<string>>>();

        private const string LogName = "Bus";
        private static readonly bool Initialized = InitializePipe();
        private static PipeServer _server;
        private static PipeClient _client;

        /// <summary>
        /// Initiate the pipe that connects to all other FOG bus instances
        /// It MUST be assumed that this pipe is compromised
        /// Do NOT send security relevant data across it
        /// </summary>
        /// <returns></returns>
        private static bool InitializePipe()
        {
            // Attempt to become the pipe server
            try
            {
                _server = new PipeServer("fog-bus");
                _server.MessageReceived += pipe_RecieveMessage;
                _server.Start();
                Log.Entry(LogName, "Became bus server");

                return true;
            } catch (Exception) {}

            // If someone else is already a pipe server, try and become a pipe client
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

        /// <summary>
        /// Send a message to other bus instances
        /// </summary>
        /// <param name="msg">The message to send, should follow the define format</param>
        private static void SendMessage(string msg)
        {
            if (!Initialized) return;

            if(_server != null && _server.IsRunning())
                _server.SendMessage(msg);

            if (_client != null && _client.IsConnected())
                _client.SendMessage(msg);
        }

        /// <summary>
        /// Emit a message to all listeners
        /// </summary>
        /// <param name="channel">The channel to emit on</param>
        /// <param name="data">The data to send</param>
        /// <param name="global">Should the data be sent to other instances</param>
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

        /// <summary>
        /// Register an action with a channel
        /// </summary>
        /// <param name="channel">The channel to register within</param>
        /// <param name="action">The action (method) to register</param>
        public static void Register(Channel channel, Action<string> action)
        {
            Log.Entry(LogName, string.Format("Registering {0} in channel {1}", action.Method.Name, channel));

            if (!Registrar.ContainsKey(channel))
                Registrar.Add(channel, new LinkedList<Action<string>>());
            if (Registrar[channel].Contains(action)) return;

            Registrar[channel].AddLast(action);
        }

        /// <summary>
        /// Unregister an action from a pipe
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="action"></param>
        public static void UnRegister(Channel channel, Action<string> action)
        {
            Log.Entry(LogName, string.Format("UnRegistering {0} in channel {1}", action.Method.Name, channel));

            if (!Registrar.ContainsKey(channel)) return;
            Registrar[channel].Remove(action);
        }

        /// <summary>
        /// Called when the server pipe recieves a message
        /// It will replay the message to all other instances, including the original sender
        /// </summary>
        /// <param name="client">The instance who initiated the message</param>
        /// <param name="message">The formatted event</param>
        private static void pipe_RecieveMessage(Client client, string message)
        {
            EmitMessageFromPipe(message);
            SendMessage(message);
        }

        /// <summary>
        /// Called when the pipe client recieves a message
        /// </summary>
        /// <param name="message"></param>
        private static void pipe_RecieveMessage(string message)
        {
            EmitMessageFromPipe(message);
        }

        /// <summary>
        /// Parse a message recieved in the pipe and emit it to channels confined in its instance
        /// </summary>
        /// <param name="message"></param>
        private static void EmitMessageFromPipe(string message)
        {
            try
            {
                var rawChannel = message.Substring(0, message.IndexOf("//"));
                var channel = (Channel) Enum.Parse(typeof(Channel), rawChannel);
                var data = message.Remove(rawChannel.Length+2);
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