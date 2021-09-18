using MySql.Data.MySqlClient;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ThestralServer
{
    class DatabaseInterface
    {
        //TODO: make all db calls async
        private static RestClient client;
        private static string write;
        internal static void ConnectToDatabase(string phpAddress, string writePw)
        {
            client = new RestClient(phpAddress);
            //TODO: add authentication
            // client.Authenticator = new HttpBasicAuthenticator(username, password);
            write = writePw;
        }

        internal static uint? GetClientUserID(uint clientId, string accountName)
        {
            RestRequest request = new RestRequest("get_client_connected.php");
            request.AddParameter("name", accountName);
            request.AddParameter("client_id", clientId);
            request.AddParameter("password", write);
            var response = client.Post<uint>(request);
            uint val;
            string[] tokens = response.Content.Split('|');
            if (uint.TryParse(tokens[0], out val))
            {
                if(val == clientId)
                return uint.Parse(tokens[1]);
            }
            return null;
        }

        internal static void DisconnectClient(uint clientId)
        {
            RestRequest request = new RestRequest("disconnect_user.php");
            request.AddParameter("client_id", clientId);
            request.AddParameter("password", write);
            client.Post<uint>(request);
        }

        internal static void SetChar(uint userId, uint charId, string c)
        {
            RestRequest request = new RestRequest("set_char_string.php");
            request.AddParameter("user_id", userId);
            request.AddParameter("char_id", charId);
            request.AddParameter("character_string", c);
            request.AddParameter("password", write);
            var response = client.Post<uint>(request);
            Program.Log($"Sent character string for char id {charId} to {c}, got response: {response.Content}", LogType.Database_Status);
        }

        internal static string GetCharAppearance(uint userId, uint charId)
        {
            RestRequest request = new RestRequest("get_char_string.php");
            request.AddParameter("user_id", userId);
            request.AddParameter("char_id", charId);
            request.AddParameter("password", write);
            var response = client.Post<uint>(request);
            string s = response.Content.Trim();
            if(s == "null" || s == "insufficient access")
            {
                s = "0,1,9,2,3,4/m,4/Tail,76,76,76,Body,164,164,164,Mane,76,76,76,Eyes,102,102,102,Wings,137,137,137";
            }
            return s;
        }
    }
}
