using ClientClass;
using Crypto;
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

        static void Main(string[] args)
        {
            Console.Title = "Server";
            Task.Run(() => InterfaceUtil.Start());
            

            // Task.Run(() => TCPConnectionSetup());
            Task.Run(() => ServerUtil.Init());

            #region Logovanje klijenta na server

            Socket serverSocketUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50000);
            serverSocketUDP.Bind(serverEP);

            // Console.WriteLine($"Server initialized and waiting on: {serverEP}");
            Logger.Log($"Server initialized and waiting on: {serverEP}");
            

            byte[] buffer = new byte[5000];         //buffer za prijem poruka od klijenta
            int numBytes;

            while (true)
            {
                try
                {
                    EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
                    numBytes = serverSocketUDP.ReceiveFrom(buffer, ref clientEP);
                    byte[] recieved = buffer.Take(numBytes).ToArray();
                    byte[] dekriptovani = Vizner.Decrypt(recieved); 
                    string message = Encoding.UTF8.GetString(dekriptovani);
                    

                    var client = ClientHandler.clients.Values.FirstOrDefault(c => c.EndPoint.Equals(clientEP));

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
                    // Console.WriteLine($"Socket error: {e.Message}");
                    Logger.Log($"Socket error: {e.Message}");
                }
                /*
                Console.Write(new string('=', 10));
                Console.Write(" CURRENT SERVER STATE ");
                Console.WriteLine(new string('=', 10));
                PrintClients(ClientHandler.clients.Values.ToList()); */
                Logger.Log(new string('=', 10));
                Logger.Log(" CURRENT SERVER STATE ");
                Logger.Log(new string('=', 10));

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
                sendMessage = Vizner.Encrypt(sendMessage);
                serverSocketUDP.SendTo(sendMessage, 0, sendMessage.Length, SocketFlags.None, clientEP);
                return;
            }

            string username = parts[0];
            string password = parts[1];

            if (ClientHandler.clients.ContainsKey(username) && ClientHandler.clients[username].Status != Status.Offline)
            {
                sendMessage = Encoding.UTF8.GetBytes($"User {username} is already logged in!");
                sendMessage = Vizner.Encrypt(sendMessage);
                serverSocketUDP.SendTo(sendMessage, 0, sendMessage.Length, SocketFlags.None, clientEP);
                return;
            }

            else if(ClientHandler.clients.ContainsKey(username) && ClientHandler.clients[username].Status == Status.Offline)
            {
                if (ClientHandler.clients[username].Password != password)
                {
                    sendMessage = Encoding.UTF8.GetBytes($"Wrong password!");
                    sendMessage = Vizner.Encrypt(sendMessage);
                    serverSocketUDP.SendTo(sendMessage, 0, sendMessage.Length, SocketFlags.None, clientEP);
                    return;
                }
            }

            var newClient = new ClientData(username, password)
            {
                EndPoint = clientEP,
                Status = Status.Idle,
                TimeLoggedIn = DateTime.Now
            };

            ClientHandler.clients[username] = newClient;


            sendMessage = Encoding.UTF8.GetBytes("Welcome to the Instant-messaging server!");
            sendMessage = Vizner.Encrypt(sendMessage);
            serverSocketUDP.SendTo(sendMessage, 0, sendMessage.Length, SocketFlags.None, clientEP);

            // Posalji listu kanala
            sendMessage = Encoding.UTF8.GetBytes("Choose a channel you would like to use:\n" + string.Join("\n", ChannelHandler.channels.Select((c, i) => $"{i + 1}. {c.name} ({c.getUnread(username)})")));
            sendMessage = Vizner.Encrypt(sendMessage);
            serverSocketUDP.SendTo(sendMessage, 0, sendMessage.Length, SocketFlags.None, clientEP);

            // Console.WriteLine($"New user  {username}:{clientEP}  has logged in.");
            Logger.Log($"New user  {username}:{clientEP}  has logged in.");
        }

        private static void HandleChannelSelection(ClientData client, string message, Socket serverSocketUDP, string username)
        {
            byte[] sendMessage;
            if (!int.TryParse(message.Trim(), out int index) || index < 1 || index > ChannelHandler.channels.Count)
            {
                sendMessage = Encoding.UTF8.GetBytes("Invalid option. Please choose again.\n" + string.Join("\n", ChannelHandler.channels.Select((c, i) => $"{i + 1}. {c.name} ({c.getUnread(username)})")));
                sendMessage = Vizner.Encrypt(sendMessage);
                serverSocketUDP.SendTo(sendMessage, 0, sendMessage.Length, SocketFlags.None, client.EndPoint);
                return;
            }

            ChannelHandler.channels[index - 1].setUnread(username);
            client.ActiveOnChannel = ChannelHandler.channels[index - 1].name;
            client.Status = Status.Online;

            sendMessage = Encoding.UTF8.GetBytes($"Successfully joined {client.ActiveOnChannel} channel! Start chatting now!");
            sendMessage = Vizner.Encrypt(sendMessage);
            serverSocketUDP.SendTo(sendMessage, 0, sendMessage.Length, SocketFlags.None, client.EndPoint);

            // Console.WriteLine($"User  {client.Username}  joined  {client.ActiveOnChannel}  channel.");
            Logger.Log($"User  {client.Username}  joined  {client.ActiveOnChannel}  channel.");
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
                    msg = Vizner.Encrypt(msg);
                    serverSocketUDP.SendTo(msg, 0, msg.Length, SocketFlags.None, client.EndPoint);
                    break;
            }

        }
    }
}
