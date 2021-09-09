using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ThestralServer
{
    class Client
    {
        private readonly TcpClient client;
        private readonly NetworkStream stream;
        internal uint clientId;
        internal bool connectedToInstance = false;
        private uint instanceId;
        internal bool awaitingValidation;

        internal string DisplayName { get; set; }

        public Client(TcpClient client)
        {
            this.client = client;
            awaitingValidation = true;
            stream = client.GetStream();
        }

        internal bool IsConnected()
        {
            return client.Connected;
        }

        internal Message[] GetMessages()
        {
            List<Message> messages = new List<Message>();
            if(client.Available > 0)
            {
                byte[] bytes = new byte[client.Available];
                int byteCount = stream.Read(bytes, 0, bytes.Length);
                string[] s = Encoding.ASCII.GetString(bytes, 0, byteCount).Split('\n');
                for(int i = 0; i < s.Length - 1; i++)
                    messages.Add(Message.FromString(s[i]));
            }
            return messages.ToArray();
        }

        internal void SendMessage(byte[] vs)
        {
            stream.Write(vs);
        }

        internal uint GetInstance()
        {
            return instanceId;
        }

        internal void SetInstance(uint instanceId)
        {
            this.instanceId = instanceId;
            connectedToInstance = true;
        }
    }
}
