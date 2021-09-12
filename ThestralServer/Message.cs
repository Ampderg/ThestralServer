using System;
using System.Collections.Generic;
using System.Text;

namespace ThestralServer
{
    class Message
    {
        internal double priority { get; private set; }
        internal bool onTick { get; private set; }
        internal string message { get; private set; }
        internal uint id { get; private set; }
        internal bool hasId { get; private set; }

        internal static Message FromString(string input)
        {
            try
            {
                Message m = new Message();

                string[] s = input.Split(new char[] { ':' }, 2);
                if (!string.IsNullOrWhiteSpace(s[0]))
                {
                    m.id = uint.Parse(s[0]);
                    m.hasId = true;
                }
                else
                    m.hasId = false;
                m.message = s[1];

                return m;
            }
            catch(FormatException)
            {
                Program.Log("Incoming message was in the wrong format: " + input, LogType.Debug);
                return null;
            }
        }
    }
}
