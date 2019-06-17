using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using Listserver.Databases;

namespace Listserver
{
    class Program
    {
        public static Database DB;
        public static Config   Config;
        public static string   ServerList;
        public static string   StartupPath;
        public static bool     DisableLogin = false;

        static void Main(string[] args)
        {
            bool bDatabaseOK = false;

            /* Show a little introduction text. */
            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine("Graal 2.1.5 List Server (Created by Seipheroth)");
            Console.WriteLine("-----------------------------------------------\n");

            /* Get the startup path. */
            StartupPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);

            /* Load the listserver config. */
            Config = new Config("config/listserver.cfg");

            if (Config.GetBool("disablelogin")) DisableLogin = true;

            string sDBType = Config.Get("dbtype").ToLower();
            switch (sDBType)
            {
                /* Try to create a connection with the MySQL server. */
                default:
                case "mysql":
                    DB = new MySQL();
                    bDatabaseOK = ((MySQL)DB).Connect(Config["mysql_hostname"], Config["mysql_username"], Config["mysql_password"], Config["mysql_database"]);
                    Log.Write(LogLevel.Success, "Server", "Connected to MySQL server.");
                    break;

                /* Use a text database */
                case "file":
                case "text":
                case "textdb":
                    DB = new TextDB();
                    bDatabaseOK = ((TextDB)DB).Init();
                    Log.Write(LogLevel.Success, "Server", "Text database initialized.");
                    break;
            }


            if (bDatabaseOK)
            {
                /* Get the port number from the config file. */
                int iPort = 21555;
                if (Config.Contains("port"))
                {
                    try
                    {
                        iPort = Config.GetInt("port");
                    }
                    catch 
                    {
                        Log.Write(LogLevel.Error, "Config", "Invalid port '{0}'.", Config["port"]);
                    }
                    if (iPort == 0) iPort = 21555;
                }

                /* Start the listserver. */
                ServerList = DB.GetServers();

                Server oServer = new Server(iPort);

            }
            else
            {

                /* Failed to connect to the MySQL server. */
                Console.WriteLine("\nPress any key to continue.");
                Console.ReadKey();

            }
        }
    }
}