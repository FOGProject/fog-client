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
using SuperWebSocket;
using WebSocket4Net;

namespace FOG.Core
{
    /// <summary>
    ///     An event driven IPC interface. Can also be used to send event only within the running process.
    /// </summary>
    public static class Bus
    {
        /// <summary>
        ///     Channels are used to categorize events.
        /// </summary>
        public enum Channel
        {
            Debug,
            Power,
            Notification,
            Status,
            Update
        }

        /// <summary>
        ///     The role of this bus instance. This is only needed for IPC. Note that the Server bus must be initialized before a
        ///     client bus.
        /// </summary>
        public enum Mode
        {
            Server,
            Client
        }

        private const string LogName = "Bus";
        private const int Port = 1277;

        private static readonly Dictionary<Channel, LinkedList<Action<dynamic>>> Registrar =
            new Dictionary<Channel, LinkedList<Action<dynamic>>>();

        private static bool _initialized;
        private static BusServer _server;
        private static BusClient _client;
        private static Mode _mode = Mode.Client;

        /// <summary>
        ///     Set the mode of the bus instance. Upon calling this method, the IPC interface will initialize.
        /// </summary>
        /// <param name="mode"></param>
        public static void SetMode(Mode mode)
        {
            _mode = mode;
            Initializesocket();
        }

        /// <summary>
        ///     Initiate the socket that connects to all other FOG bus instances
        ///     It MUST be assumed that this socket is compromised
        ///     Do NOT send security relevant data across it
        /// </summary>
        /// <returns></returns>
        private static void Initializesocket()
        {
            switch (_mode)
            {
                case Mode.Server:
                    // Attempt to become the socket server
                    try
                    {
                        _server = new BusServer();
                        _server.Socket.NewMessageReceived += socket_RecieveMessage;
                        _server.Start();
                        Log.Entry(LogName, "Became bus server");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(LogName, "Could not enter socket");
                        Log.Error(LogName, ex);
                    }
                    break;
                case Mode.Client:
                    try
                    {
                        _client = new BusClient(Port);
                        _client.Socket.MessageReceived += socket_RecieveMessage;
                        _client.Start();
                        Log.Entry(LogName, "Became bus client");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(LogName, "Could not enter socket");
                        Log.Error(LogName, ex);
                    }
                    break;
            }
            _initialized = true;
        }

        /// <summary>
        ///     Send a message to other bus instances
        /// </summary>
        /// <param name="msg">The message to send, should be in json format</param>
        private static void SendMessage(string msg)
        {
            if (!_initialized) Initializesocket();
            if (!_initialized) return;

            if (_server != null)
                _server.Send(msg);
            else if (_client != null)
                _client.Send(msg);
        }

        /// <summary>
        ///     Emit a message to all listeners
        /// </summary>
        /// <param name="channel">The channel to emit on</param>
        /// <param name="data">The data to send</param>
        /// <param name="global">Should the data be sent to other processes</param>
        public static void Emit(Channel channel, JObject data, bool global = false)
        {
            Emit(channel, data.ToString(), global);
        }

        /// <summary>
        ///     Emit a message to all listeners
        /// </summary>
        /// <param name="channel">The channel to emit on</param>
        /// <param name="data">The data to send</param>
        /// <param name="global">Should the data be sent to other processes</param>
        private static void Emit(Channel channel, string data, bool global = false)
        {
            if (global)
            {
                var transport = new JObject {{"channel", channel.ToString()}, {"data", data}};
                Log.Entry(LogName, transport.ToString());
                SendMessage(transport.ToString());

                // If this bus instance is a client, wait for the event to be bounced-back before processing
                if (_client != null)
                    return;
            }

            Log.Entry(LogName, "Emmiting message on channel: " + channel);

            if (!Registrar.ContainsKey(channel)) return;
            try
            {
                var json = JObject.Parse(data);
                foreach (var action in Registrar[channel])
                    action.Invoke(json);
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Unable to parse data");
                Log.Error(LogName, ex);
            }
        }

        /// <summary>
        ///     Register an action with a channel. When a message is recieved on this channel, the method will be called.
        /// </summary>
        /// <param name="channel">The channel to register within</param>
        /// <param name="action">The action (method) to register</param>
        public static void Subscribe(Channel channel, Action<dynamic> action)
        {
            Log.Entry(LogName, $"Registering {action.Method.Name} in channel {channel}");

            if (!Registrar.ContainsKey(channel))
                Registrar.Add(channel, new LinkedList<Action<dynamic>>());
            if (Registrar[channel].Contains(action)) return;

            Registrar[channel].AddLast(action);
        }

        /// <summary>
        ///     Unregister an action from a channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="action"></param>
        public static void Unsubscribe(Channel channel, Action<dynamic> action)
        {
            Log.Entry(LogName, $"UnRegistering {action.Method.Name} in channel {channel}");

            if (!Registrar.ContainsKey(channel)) return;
            Registrar[channel].Remove(action);
        }

        /// <summary>
        ///     Called when the server socket recieves a message
        ///     It will replay the message to all other instances, including the original sender unless told otherwise
        /// </summary>
        private static void socket_RecieveMessage(object sender, MessageReceivedEventArgs messageReceivedEventArgs)
        {
            EmitMessageFromSocket(messageReceivedEventArgs.Message);
        }

        /// <summary>
        ///     Called when the socket client recieves a message
        /// </summary>
        private static void socket_RecieveMessage(WebSocketSession session, string value)
        {
            EmitMessageFromSocket(value);
        }

        /// <summary>
        ///     Parse a message recieved in the socket and emit it to channels confined in its instance
        /// </summary>
        /// <param name="message"></param>
        private static void EmitMessageFromSocket(string message)
        {
            try
            {
                dynamic transport = JObject.Parse(message);

                var channel = (Channel) Enum.Parse(typeof (Channel), transport.channel.ToString());
                Emit(channel, transport.data.ToString(), transport.bounce != null && !transport.bounce);
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not parse message from socket");
                Log.Error(LogName, ex);
            }
        }

        public static void Dispose()
        {
            if (!_initialized) return;

            switch (_mode)
            {
                case Mode.Client:
                    _client.Stop();
                    break;
                case Mode.Server:
                    _server.Stop();
                    break;
            }
        }
    }
}