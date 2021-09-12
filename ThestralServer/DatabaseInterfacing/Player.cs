using System;
using System.Collections.Generic;
using System.Text;

namespace ThestralServer.DatabaseInterfacing
{
    class Player
    {
        internal uint playerId;
        internal string accountName;
        internal string displayName;

        internal bool LogIn(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            displayName = accountName = username;
            return true;
        }
    }
}
