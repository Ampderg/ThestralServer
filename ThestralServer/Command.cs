using System;
using System.Collections.Generic;
using System.Text;

namespace ThestralServer
{
    class Command
    {
        public uint clientId { get; private set; }
        public uint messageId { get; private set; }
        public string command { get; private set; }
        public string[] parameters { get; private set; }
        internal static Command FromMessage(uint clientId, uint messageId, string message)
        {
            Command c = new Command();
            c.clientId = clientId;
            c.messageId = messageId;
            string[] cmdtokens = message.Split('|', 2);
            c.command = cmdtokens[0];
            if (cmdtokens.Length > 1)
                c.parameters = cmdtokens[1].Split('|');
            else
                c.parameters = new string[0];
            return c;
        }

        public override string ToString()
        {
            return string.Format("[{0}] /{1}:{2}|{3}", clientId, messageId, command, string.Join('|', parameters));
        }
    }
}
