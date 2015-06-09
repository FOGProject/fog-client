/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2015 FOG Project
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 3
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace FOG.Handlers
{
    public static class Bus
    {
        public enum Channel
        {
            Debug,
            Power,
            Notification,
            Status,
            Update
        }

        public enum Mode
        {
            Server,
            Client
        }

        private static readonly Dictionary<Channel, LinkedList<Action<string>>> Registrar =
            new Dictionary<Channel, LinkedList<Action<string>>>();

        private const string LogName = "Bus";
        private static bool _initialized = false;
        private static PipeServer _server;
        private static PipeClient _client;

        private static Mode _mode = Mode.Client;

        public static void SetMode(Mode mode)
        {
            _mode = mode;
            InitializePipe();
        }


        /// <summary>
        /// Initiate the pipe that connects to all other FOG bus instances
        /// It MUST be assumed that this pipe is compromised
        /// Do NOT send security relevant data across it
        /// </summary>
        /// <returns></returns>
        private static void InitializePipe()
        {

            switch (_mode)
            {
                case Mode.Server:
                    // Attempt to become the pipe server
                    try
                    {
                        _server = new PipeServer("fog-bus");
                        _server.MessageReceived += pipe_RecieveMessage;
                        _server.Start();
                        Log.Entry(LogName, "Became bus server");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(LogName, "Could not enter named pipe");
                        Log.Error(LogName, ex);
                    }
                    break;
                case Mode.Client:
                    // If someone else is already a pipe server, try and become a pipe client
                    try
                    {
                        _client = new PipeClient("fog-bus");
                        _client.MessageReceived += pipe_RecieveMessage;
                        _client.Connect();
                        Log.Entry(LogName, "Became bus client");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(LogName, "Could not enter named pipe");
                        Log.Error(LogName, ex);
                    }
                    break;
            }
            _initialized = true;
        }

        /// <summary>
        /// Send a message to other bus instances
        /// </summary>
        /// <param name="msg">The message to send, should follow the define format</param>
        private static void SendMessage(string msg)
        {
            if (!_initialized) InitializePipe();
            if (!_initialized) return;

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
        public static void Emit(Channel channel, JObject data, bool global = false)
        {
            Emit(channel, data.ToString(), global);
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
                var transport = new JObject {{"channel", channel.ToString()}, {"data", data}};
                SendMessage(transport.ToString());

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
        public static void Subscribe(Channel channel, Action<string> action)
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
        public static void Unsubscribe(Channel channel, Action<string> action)
        {
            Log.Entry(LogName, string.Format("UnRegistering {0} in channel {1}", action.Method.Name, channel));

            if (!Registrar.ContainsKey(channel)) return;
            Registrar[channel].Remove(action);
        }

        /// <summary>
        /// Called when the server pipe recieves a message
        /// It will replay the message to all other instances, including the original sender unless told otherwise
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
                var transport = JObject.Parse(message);
                var channel = (Channel)Enum.Parse(typeof(Channel), transport["channel"].ToString());
                Emit(channel, transport["data"].ToString());
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not parse message from pipe");
                Log.Error(LogName, ex);

            }
        }

        public static void Dispose()
        {
            if(_initialized && _mode == Mode.Client)
                _client.Kill();
        }

    }
}