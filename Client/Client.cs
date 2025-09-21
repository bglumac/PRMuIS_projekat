using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Sockets;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Client.Classes;

namespace Client
{
    internal class Client
    {
        static void Main(string[] args)
        {

            #region Konekcija na server
            Console.Title = "Client";

            Console.WriteLine("Instant-messaging application! WELCOME!");
            Console.WriteLine("Log in to start chattng!");

            ServerUtil.Init();
            InterfaceUtil.Start();

            #endregion
        }
    }
}
