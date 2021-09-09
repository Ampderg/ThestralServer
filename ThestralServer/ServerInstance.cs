using System;
using System.Collections.Generic;
using System.Text;
using ThestralServer.Exceptions;

namespace ThestralServer
{
    class ServerInstance
    {
        private Dictionary<uint, Entity> entityInstances;
        private IdAssigner entityInstanceIds;
        internal Dictionary<uint, Client> connectedClients;
        internal int playerCountLimit = 1000;
        internal int sceneId;
        internal uint instanceId;
        internal Server parentServer;

        public ServerInstance()
        {
            entityInstances = new Dictionary<uint, Entity>();
            entityInstanceIds = new IdAssigner();
            connectedClients = new Dictionary<uint, Client>();
        }

        internal uint CreateEntity(uint entityTypeId)
        {
            Entity e = new Entity();
            e.EntityTypeId = entityTypeId;
            e.OverrideMaxMoveSpeed = EntityLibrary.entries[entityTypeId].maxMoveSpeed;
            uint id = entityInstanceIds.GetFreeID();
            entityInstances[id] = e;
            parentServer.Log($"Created Entity of type {entityTypeId} with entity instance id: {id} on server instance: {instanceId}", LogType.Entity_Status);
            return id;
        }

        internal void DestroyEntity(uint entityInstanceId)
        {
            entityInstances.Remove(entityInstanceId);
            entityInstanceIds.UnassignID(entityInstanceId);
            foreach(var pair in connectedClients)
            {
                parentServer.DestroyEntityForClient(entityInstanceId, pair.Key);
            }
            parentServer.Log($"Destroying Entity with instance id: {entityInstanceId}", LogType.Entity_Status);
        }

        internal void GrantAuthority(uint entityInstanceId, uint clientId)
        {
            entityInstances[entityInstanceId].SetAuthority(clientId);
            parentServer.Log($"Granting client {clientId} authority over entity {entityInstanceId}");
        }

        internal bool DoesClientHaveAuthorityOverEntity(uint clientId, uint entityInstanceId)
        {
            if (!entityInstances.ContainsKey(entityInstanceId))
                return false;
            return entityInstances[entityInstanceId].DoesClientHaveAuthority(clientId);
        }

        internal void ConnectClient(uint clientId, Client c)
        {
            connectedClients[clientId] = c;
        }

        internal void DisconnectClient(uint clientId)
        {
            parentServer.Log($"Disconnecting client {clientId} from Instance {instanceId}", LogType.Client_Status);
            for (uint i = 0; i < entityInstances.Count; i++)
            {
                if(entityInstances.ContainsKey(i) && entityInstances[i].DoesClientHaveAuthority(clientId))
                {
                    DestroyEntity(i);
                    i--;
                }
                    
            }
            connectedClients.Remove(clientId);
            if(connectedClients.Count == 0)
            {
                parentServer.KillInstance(instanceId);
            }
        }

        internal int[] MoveEntity(uint entityInstanceId, uint clientId, int xPixel, int yPixel)
        {
            if(DoesClientHaveAuthorityOverEntity(clientId, entityInstanceId))
            {
                return entityInstances[entityInstanceId].MoveEntity(xPixel, yPixel);
            }
            parentServer.Log($"Entity {entityInstanceId} attempted to move by client {clientId}", LogType.Warning);
            return null;
        }

        internal int[] GetEntityVelocity(uint entityInstanceId)
        {
            return entityInstances[entityInstanceId].GetVelocity();
        }

        internal string[] PopulateCurrentEntitiesCommandStrings()
        {
            string[] s = new string[entityInstances.Count];
            int i = 0;
            foreach(var pair in entityInstances)
            {
                s[i] = Program.FormatCommand("createEntity",
                    pair.Value.EntityTypeId.ToString(), pair.Value.PixelX.ToString(), pair.Value.PixelY.ToString(),
                    false.ToString(), instanceId.ToString(), pair.Key.ToString());
                parentServer.Log("Populated instance command: " + s[i]);
                i++;
            }
            return s;
        }
    }
}
