using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace ThestralServer
{
    //build command:  dotnet publish -c release -r ubuntu.16.04-x64 --self-contained
    class Program
    {

        public const int PixelsPerUnit = 20;
        public static int logLevel = 0;

        static void Main(string[] args)
        {
            Log("---ThestralServer v0.1.0---", LogType.Hosting_Info);


            Log("Populating Entity Library");
            EntityLibrary.Populate();

            Log("Populating Room Collision Data");
            RoomCollision.Initialize();

            

            List<Server> servers = new List<Server>();

            string php = "server=localhost;user=root;database=THES;port=3306;password=password";
            string writePw = "";
            if (args.Length >= 5)
            {
                php = args[3];
                writePw = args[4];
            }

            Log("Connecting to database");
            DatabaseInterface.ConnectToDatabase(php, writePw);

            if (args.Length >= 3)
                logLevel = int.Parse(args[2]);
            if(args.Length >= 2)
                servers.Add(CreateServer(args[0], 0, args[1]));
            else if(args.Length == 1)
                servers.Add(CreateServer(args[0]));
            else
                servers.Add(CreateServer());



            bool shutdown = false;
            while (!shutdown)
            {
                if (Console.KeyAvailable)
                {
                    var s = Console.ReadKey(true);
                    switch (s.Key)
                    {
                        case ConsoleKey.Escape:
                        case ConsoleKey.E:
                            Log("Beginning host shutdown", LogType.Hosting_Info);
                            shutdown = true;
                            //process all server shutdown code
                            for (int i = 0; i < servers.Count; i++)
                            {
                                servers[i].StopServer();
                            }
                            break;
                    }
                }
            }
            Log("Host shut down, press enter to close window.", LogType.Hosting_Info);
            Console.ReadLine();
        }

        private static Server CreateServer(string ipAddress = "", int port = 0, string portString = "")
        {
            Log("Obtaining new server port");
            IPAddress ip;
            while (!IPAddress.TryParse(ipAddress, out ip))
            {
                Console.Write("Please enter the desired IP Address: ");
                ipAddress = Console.ReadLine();
            }
            if (port == 0)
            {
                if (!(portString != "" && int.TryParse(portString, out port)))
                {
                    portString = "t";
                    while (!int.TryParse(portString, out port))
                    {
                        Console.Write("Please enter the desired port: ");
                        portString = Console.ReadLine();
                    }
                }
            }
            Log("Creating server");
            Server s = new Server();
            s.StartServer(ip, port, "Server");

            return s;
        }

        //color key:
        //DarkGray - default info
        //White - host status / host input
        //Blue - client connection
        //Cyan - server status
        //Red - server error
        public static void Log(string text, LogType logType = LogType.Info)
        {
            if ((int)logType < logLevel)
                return;
            ConsoleColor color = ConsoleColor.Gray;
            switch(logType)
            {
                case LogType.Debug:
                    color = ConsoleColor.DarkGray;
                    break;
                case LogType.Chat:
                    color = ConsoleColor.DarkCyan;
                    break;
                case LogType.Client_Status:
                    color = ConsoleColor.Blue;
                    break;
                case LogType.Server_Status:
                    color = ConsoleColor.Cyan;
                    break;
                case LogType.Warning:
                    color = ConsoleColor.Yellow;
                    break;
                case LogType.Error:
                    color = ConsoleColor.Red;
                    break;
                case LogType.Hosting_Info:
                    color = ConsoleColor.White;
                    break;
                case LogType.Entity_Status:
                    color = ConsoleColor.DarkBlue;
                    break;
                case LogType.Database_Status:
                    color = ConsoleColor.Green;
                    break;
            }
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static string FormatCommand(string command, params string[] pars)
        {
            string s = command;
            foreach(string p in pars)
            {
                s += "|" + p;
            }
            return s;
        }
    }

    public enum LogType
    {
        Debug = 0,
        Chat = 4,
        Info = 5,
        Hosting_Info = 99,
        Client_Status = 15,
        Server_Status = 90,
        Entity_Status = 10,
        Warning = 50,
        Error = 100,
        Database_Status = 75
    }
}
