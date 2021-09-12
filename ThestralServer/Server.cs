using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ThestralServer
{
    class Server
    {
        //Server Info
        private TcpListener server;
        private string serverName;

        //Client Connection
        public Dictionary<uint, Client> connectedClients;
        private IdAssigner clientIdAssigner;
        private Task acceptConnectionTask;

        //Server Loop
        internal const double TicksPerSecond = 20;
        internal double realTicksPerSecond = TicksPerSecond;
        private double msPerTick { get { return 1000.0 / realTicksPerSecond; } }
        private Stopwatch frameStopwatch;
        private Timer frameTimer;

        //Client Messages
        private Queue<Command> tickQueue;
        private uint messageAcceptClientIdOffset = 0;

        //Server Instances
        private Dictionary<uint, ServerInstance> instances;
        private IdAssigner instanceIdAssigner;

        //Database Connection
        //private DatabaseInterface db;

        internal void Log(string text, LogType logType = LogType.Info)
        {
            Program.Log("[" + serverName + ", " + DateTime.Now.ToString() + " " + DateTime.Now.Millisecond + "] " + text, logType);
        }

        #region Server State Management
        internal void StartServer(IPAddress ip, int port, string serverName)
        {
            this.serverName = serverName;
            server = new TcpListener(ip, port);
            Log("Created server listener for port " + port);
            server.Start();
            Log("Server started on address " + ip + " and port " + port, LogType.Server_Status);
            connectedClients = new Dictionary<uint, Client>();
            Log("Set empty client list");
            tickQueue = new Queue<Command>();
            Log("Set empty command queue");
            frameStopwatch = new Stopwatch();
            frameTimer = new Timer(msPerTick);
            frameTimer.Elapsed += TickElapsed;
            frameTimer.Start();
            instances = new Dictionary<uint, ServerInstance>();
            instanceIdAssigner = new IdAssigner();
            clientIdAssigner = new IdAssigner();
        }

        internal void StopServer()
        {
            //TODO: kick clients from server
            foreach(var pair in connectedClients)
            {
                SendClientMessage(pair.Key, 0, Program.FormatCommand("forceDisconnect"), false);
            }

            frameStopwatch.Start();
            frameTimer.Stop();
            server.Stop();
            Log("Successfully shut down server", LogType.Server_Status);
        }
        #endregion

        #region Server Loop Flow
        private void TickElapsed(object sender, ElapsedEventArgs e)
        {
            ProcessServerFrame();
        }

        internal void ProcessServerFrame()
        {
            frameStopwatch.Restart();
            //Log("Frame start!");
            TryAcceptConnection();
            AcceptClientMessages();
            ProcessClientMessages();
            //Log("Frame duration: " + frameStopwatch.ElapsedMilliseconds);
        }
        #endregion

        #region Client Connection
        private bool TryAcceptConnection()
        {
            if (acceptConnectionTask == null && server.Pending())
            {
                Log("Found pending connection");
                acceptConnectionTask = Task.Run(() => AcceptPendingConnection());
                return true;
            }
            return false;
        }
        //accept new connections and add them to the connected client list
        private void AcceptPendingConnection()
        {
            try
            {
                //Accept the connection
                Log("Accepting client");
                TcpClient client = server.AcceptTcpClient();
                Log("Client accepted");

                //Validate the client data

                //Create client on server
                Client c = new Client(client);
                c.clientId = clientIdAssigner.GetFreeID();
                Log("Client container created");
                connectedClients[c.clientId] = c;
                Log("Client [" + client.Client.RemoteEndPoint.ToString() + "] successfully added to connected clients list", LogType.Client_Status);
            }
            catch (Exception e)
            {
                Log("ERR: Client failed to connect!" + Environment.NewLine + e.StackTrace, LogType.Error);
            }
            acceptConnectionTask = null;
        }

        private void DisconnectClient(uint clientId)
        {
            if(!connectedClients.ContainsKey(clientId))
            {
                Log($"Tried to disconnect client {clientId}, but could not find them in connected clients.", LogType.Warning);
                return;
            }
            instances[connectedClients[clientId].GetInstance()].DisconnectClient(clientId);
            connectedClients.Remove(clientId);
            clientIdAssigner.UnassignID(clientId);
            Log($"Client {clientId} disconnected!");
        }
        #endregion

        #region Client Message Management
        //accept client messages and add them to the queue to be processed
        private void AcceptClientMessages()
        {
            uint i = 0;
            uint index = messageAcceptClientIdOffset;
            while (i <= clientIdAssigner.highestFreeId)
            {
                if (index > clientIdAssigner.highestFreeId)
                {
                    index = 0;
                }
                if (connectedClients.ContainsKey(index))
                {
                    if (!connectedClients[index].IsConnected())
                    {
                        //disconnect the client if it timed out
                        connectedClients.Remove(index);
                        i--;
                        index--;
                    }
                    Message[] messages = connectedClients[index].GetMessages();
                    if (messages != null && messages.Length > 0)
                    {
                        Log("Accepted Client Messages:", LogType.Debug);
                        foreach (Message m in messages)
                        {
                            string[] cmdTokens = m.message.Split('/');
                            for (int j = 1; j < cmdTokens.Length; j++)
                            {
                                Command c = Command.FromMessage(index, m.id, cmdTokens[j]);
                                tickQueue.Enqueue(c);
                                //tickQueue.Enqueue(c, m.priority);
                                Log(c.ToString(), LogType.Debug);
                            }
                        }
                    }
                }
                i++;
                index++;
                if (frameStopwatch.ElapsedMilliseconds > msPerTick / 2)
                {
                    break;
                }
            }
            messageAcceptClientIdOffset = index;
        }

        //process client messages
        private void ProcessClientMessages()
        {
            //while(tickQueue.HasNext() && frameStopwatch.ElapsedMilliseconds < msPerTick)
            while (tickQueue.Count > 0 && frameStopwatch.ElapsedMilliseconds < msPerTick)
            {
                Command c = tickQueue.Dequeue();
                Log("Processing client message: " + c.ToString(), LogType.Debug);
                switch (c.command)
                {
                    case "validate":
                        CmdValidateClient(c);
                        break;
                    case "joinScene":
                        //Called when a player joins a scene by scene build id
                        //TODO: add code for joining an instance, adding player character for all players, and populating
                        //the scene with all entities current in the scene for the player that is joining
                        CmdJoinScene(c);
                        break;
                    case "disconnect":
                        DisconnectClient(c.clientId);
                        break;
                    case "m":
                    case "move":
                        //move entity to desired location
                        CmdMoveEntity(c);
                        break;
                    case "say":
                        CmdChatMessage(c, "Say");
                        break;
                    default:
                        Log("Recieved invalid command: " + c.ToString(), LogType.Info);
                        break;
                }
            }
            if (frameStopwatch.ElapsedMilliseconds >= msPerTick)
                Log($"Cant keep up! Ended tick with {tickQueue.Count} commands remaining!");
        }

        private void SendClientMessage(uint clientId, uint msgId, string s, bool hasId = true)
        {
            if (connectedClients[clientId].IsConnected())
            {
                string message = string.Format("{0}:{1}\n", (hasId ? msgId.ToString() : ""), s);
                connectedClients[clientId].SendMessage(Encoding.ASCII.GetBytes(message));
                Log("Sent " + clientId + " message: " + message, LogType.Debug);
            }
            else
            {
                DisconnectClient(clientId);
            }
        }
        #endregion

        #region Commands

        //Validate the connecting client's information and send a message back to them to confirm their state.
        //Expected parameters:
        //[0] - username
        private void CmdValidateClient(Command c)
        {
            Log("Validating client: " + c.ToString());
            Client client = connectedClients[c.clientId];
            if (client.awaitingValidation)
            {
                client.awaitingValidation = false;
                client.DisplayName = c.parameters[0];
                SendClientMessage(c.clientId, c.messageId, string.Format("/validate|{0}", "Valid"));
            }
        }

        //Validate the connecting client's information and send a message back to them to confirm their state.
        //Expected parameters:
        //[0] - scene id
        private void CmdJoinScene(Command c)
        {
            //Get a free instance
            if(connectedClients[c.clientId].connectedToInstance)
            {
                instances[connectedClients[c.clientId].GetInstance()].DisconnectClient(c.clientId);
            }
            ServerInstance instance = GetFreeSceneInstance(int.Parse(c.parameters[0]));
            instance.ConnectClient(c.clientId, connectedClients[c.clientId]);
            connectedClients[c.clientId].SetInstance(instance.instanceId);
            //Send the current entity information to the player
            string[] currentEntities = instance.PopulateCurrentEntitiesCommandStrings();
            foreach(string cmd in currentEntities)
            {
                SendClientMessage(c.clientId, 0, cmd, false);
            }
            //Create the joining player character
            CreateEntity(instance.instanceId, 0, 0, 0, true, c.clientId);
        }

        //Validate the connecting client's information and send a message back to them to confirm their state.
        //Expected parameters:
        //[0] - entity instance id
        //[1] - pixel x
        //[2] - pixel y
        private void CmdMoveEntity(Command c)
        {
            uint entityInstanceId = uint.Parse(c.parameters[0]);
            int[] desiredPos = new int[2] { int.Parse(c.parameters[1]), int.Parse(c.parameters[2]) };
            int[] pos = instances[connectedClients[c.clientId].GetInstance()].MoveEntity(
                entityInstanceId, c.clientId, desiredPos[0], desiredPos[1]);
            if (pos == null) return;

            if (desiredPos[0] != pos[0] || desiredPos[1] != pos[1])
            {
                //send update to local player since their position is off
                Log($"Entity {entityInstanceId} connected to client {c.clientId} went faster than their limit, limitting speed.", LogType.Entity_Status);
                SendClientMessage(c.clientId, 0, $"/m|{entityInstanceId}|{pos[0]}|{pos[1]}", false);
            }

            //RPC update player position
            foreach(var pair in connectedClients)
            {
                if(pair.Value.clientId != c.clientId && pair.Value.connectedToInstance 
                    && pair.Value.GetInstance() == connectedClients[c.clientId].GetInstance())
                {
                    SendClientMessage(pair.Value.clientId, 0, $"/m|{entityInstanceId}|{pos[0]}|{pos[1]}|{pos[2]}|{pos[3]}", false);
                }
            }
        }

        private void CmdChatMessage(Command c, string category)
        {
            string message = string.Join(' ', c.parameters);
            message = message.Substring(0, Math.Min(message.Length, 50));

            switch (category)
            {
                case "Say":
                    foreach (var pair in connectedClients)
                    {
                        if (pair.Value.connectedToInstance
                            && pair.Value.GetInstance() == connectedClients[c.clientId].GetInstance())
                        {
                            SendClientMessage(pair.Value.clientId, 0, $"/recieveChatMsg|say|{message}", false);
                        }
                    }
                    break;
            }
        }
        #endregion

        #region Server Commands
        private ServerInstance GetFreeSceneInstance(int sceneId)
        {
            var filtered = instances.Where(e => e.Value.sceneId == sceneId);
            if (filtered.Count() == 0)
            {
                ServerInstance i = new ServerInstance();
                i.instanceId = instanceIdAssigner.GetFreeID();
                i.sceneId = sceneId;
                i.parentServer = this;
                instances.Add(i.instanceId, i);
                return i;
            }
            else return filtered.First().Value;
        }

        private void CreateEntity(uint instanceId, uint entityId, int pixelX = 0, int pixelY = 0, bool hasClientAuthority = false, uint clientAuthority = 0)
        {
            uint entityInstanceId = instances[instanceId].CreateEntity(entityId);
            if(hasClientAuthority)
                instances[instanceId].GrantAuthority(entityInstanceId, clientAuthority);

            //RPC send entity to clients
            foreach(var pair in connectedClients)
            {
                if (pair.Value.GetInstance() == instanceId)
                {
                    bool doesClientHaveAuthority = instances[instanceId].DoesClientHaveAuthorityOverEntity(pair.Value.clientId, entityInstanceId);
                    SendClientMessage(pair.Value.clientId, 0, Program.FormatCommand("createEntity",
                        entityId.ToString(), pixelX.ToString(), pixelY.ToString(), 
                        doesClientHaveAuthority.ToString(), instanceId.ToString(), entityInstanceId.ToString()));
                }
            }
        }

        internal void DestroyEntityForClient(uint entityInstanceId, uint clientId)
        {
            SendClientMessage(clientId, 0, Program.FormatCommand("destroyEntity", entityInstanceId.ToString()));
        }

        internal void KillInstance(uint instanceId)
        {
            instances.Remove(instanceId);
            instanceIdAssigner.UnassignID(instanceId);
        }
        #endregion
    }
}
