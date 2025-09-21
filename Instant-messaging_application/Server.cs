using ClientClass;
using Instant_messaging_application.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Instant_messaging_application
{
    internal class Server
    {
        public static Dictionary<string, ClientData> clients = new Dictionary<string, ClientData>();
        

        static void Main(string[] args)
        {
            Console.Title = "Server";
            Console.WriteLine("Instant-messaging application server!");

            // Task.Run(() => TCPConnectionSetup());
            Task.Run(() => ServerUtil.Init());

            #region Logovanje klijenta na server

            Socket serverSocketUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50000);
            serverSocketUDP.Bind(serverEP);

            Console.WriteLine($"Server initialized and waiting on: {serverEP}");

            byte[] buffer = new byte[5000];         //buffer za prijem poruka od klijenta
            int numBytes;

            while (true)
            {
                try
                {
                    EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
                    numBytes = serverSocketUDP.ReceiveFrom(buffer, ref clientEP);
                    string message = Encoding.UTF8.GetString(buffer, 0, numBytes);

                    var client = clients.Values.FirstOrDefault(c => c.EndPoint.Equals(clientEP));

                    if (client == null)
                    {
                        HandleLogin(clientEP, message, serverSocketUDP);
                    }
                    else
                    {
                        HandleClientMessage(client, message, serverSocketUDP);
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine($"Socket error: {e.Message}");
                }

                Console.Write(new string('=', 10));
                Console.Write(" CURRENT SERVER STATE ");
                Console.WriteLine(new string('=', 10));
                PrintClients(clients.Values.ToList()); 

            }

            #endregion
        }

        private static void HandleLogin(EndPoint clientEP, string message, Socket serverSocketUDP)
        {
            byte[] sendMessage;
            string[] parts = message.Split('|');

            if(parts.Length != 2)
            {
                sendMessage = Encoding.UTF8.GetBytes("Login format must be: username|password. Try again.");
                serverSocketUDP.SendTo(sendMessage, 0, sendMessage.Length, SocketFlags.None, clientEP);
                return;
            }

            string username = parts[0];
            string password = parts[1];

            if (clients.ContainsKey(username))
            {
                sendMessage = Encoding.UTF8.GetBytes($"User {username} is already logged in!");
                serverSocketUDP.SendTo(sendMessage, 0, sendMessage.Length, SocketFlags.None, clientEP);
                return;
            }

            var newClient = new ClientData(username, password)
            {
                EndPoint = clientEP,
                Status = Status.Idle,
                TimeLoggedIn = DateTime.Now
            };
            clients[username] = newClient;

            sendMessage = Encoding.UTF8.GetBytes("Welcome to the Instant-messaging server!");
            serverSocketUDP.SendTo(sendMessage, 0, sendMessage.Length, SocketFlags.None, clientEP);

            // Posalji listu kanala
            sendMessage = Encoding.UTF8.GetBytes("Choose a channel you would like to use:\n" + string.Join("\n", ChannelHandler.channels.Select((c, i) => $"{i + 1}. {c.name} ({c.getUnread(username)})")));
            serverSocketUDP.SendTo(sendMessage, 0, sendMessage.Length, SocketFlags.None, clientEP);

            Console.WriteLine($"New user  {username}:{clientEP}  has logged in.");
        }

        private static void HandleChannelSelection(ClientData client, string message, Socket serverSocketUDP, string username)
        {
            byte[] sendMessage;
            if (!int.TryParse(message.Trim(), out int index) || index < 1 || index > ChannelHandler.channels.Count)
            {
                sendMessage = Encoding.UTF8.GetBytes("Invalid option. Please choose again.\n" + string.Join("\n", ChannelHandler.channels.Select((c, i) => $"{i + 1}. {c.name} ({c.getUnread(username)})")));
                serverSocketUDP.SendTo(sendMessage, 0, sendMessage.Length, SocketFlags.None, client.EndPoint);
                return;
            }

            client.ActiveOnChannel = ChannelHandler.channels[index - 1].name;
            client.Status = Status.Online;

            sendMessage = Encoding.UTF8.GetBytes($"Successfully joined {client.ActiveOnChannel} channel! Start chatting now!");
            serverSocketUDP.SendTo(sendMessage, 0, sendMessage.Length, SocketFlags.None, client.EndPoint);

            Console.WriteLine($"User  {client.Username}  joined  {client.ActiveOnChannel}  channel.");
        }

        public static void HandleClientMessage(ClientData client, string message, Socket serverSocketUDP)
        {
            switch (client.Status)
            {
                case Status.Idle:
                    HandleChannelSelection(client, message, serverSocketUDP, client.Username);
                    break;
                case Status.Online:
                    // Broadcast jos nije implementiran
                    byte[] msg = Encoding.UTF8.GetBytes($"[{client.ActiveOnChannel}] {client.Username}: {message}");
                    serverSocketUDP.SendTo(msg, 0, msg.Length, SocketFlags.None, client.EndPoint);
                    break;
            }

        }

        public static void PrintClients(List<ClientData> clients)
        {
            if (clients == null || clients.Count == 0)
            {
                Console.WriteLine("No clients to display.");
                return;
            }

            Console.WriteLine("{0,-15}{1,-15}{2,-10}{3,-22}{4,-20}{5}",
                "Username", "Password", "Status", "IP Address", "Channel", "Logged in time");

            Console.WriteLine(new string('-', 100));

            string currentTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");

            foreach (var c in clients)
            {
                if (c == null) continue;

                Console.WriteLine("{0,-15}{1,-15}{2,-10}{3,-22}{4,-20}{5}",
                    c.Username,
                    c.Password,
                    c.Status,
                    c.EndPoint?.ToString() ?? "N/A",
                    c.ActiveOnChannel ?? "None",
                    c.TimeLoggedIn 
                );
            }

            Console.WriteLine(new string('-', 100));
        }
    }
}
