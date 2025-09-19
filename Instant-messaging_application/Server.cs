using ClientClass;
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
using System.Text;
using System.Threading.Tasks;

namespace Instant_messaging_application
{
    internal class Server
    {
        public static List<ClientData> clients = new List<ClientData>();
        public static List<string> chatChannels = new List<string>()
        {
            "General",
            "Gaming",
            "Chillzone",
            "Music"
        };

        static void Main(string[] args)
        {
            Console.WriteLine("Instant-messaging application server!");

            Task.Run(() => TCPConnectionSetup());

            #region Logovanje klijenta na server

            Socket serverSocketUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50000);
            serverSocketUDP.Bind(serverEP);

            Console.WriteLine($"Server initialized and waiting on: {serverEP}");

            byte[] buffer = new byte[5000];         //buffer za prijem poruka od klijenta
            byte[] sendMessage;
            int numBytes;

            while (true)
            {
                try
                {
                    Console.WriteLine("Waiting for log in....");

                    EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);

                    ClientData newClient;

                    numBytes = serverSocketUDP.ReceiveFrom(buffer, ref clientEP);
                    string message = Encoding.UTF8.GetString(buffer, 0, numBytes);

                    string[] partsOfLogin = message.Split('|');

                    if(partsOfLogin.Length != 2)
                    {
                        Console.WriteLine($"Fomrat error on {clientEP}");
                        sendMessage = Encoding.UTF8.GetBytes("The login format does not match \"username|password\" format. Try again.");
                        serverSocketUDP.SendTo(sendMessage, 0, sendMessage.Length, SocketFlags.None, clientEP);
                        continue;
                    }

                    string username = partsOfLogin[0];
                    string password = partsOfLogin[1];

                    bool alrExists = clients.Any(x =>  (x.Username == username && x.Password == password) || (x.Username == username));

                    if(alrExists)
                    {
                        sendMessage = Encoding.UTF8.GetBytes($"User {username} is already logged in!");
                        serverSocketUDP.SendTo(sendMessage, 0, sendMessage.Length, SocketFlags.None, clientEP);
                        continue;
                    }
                    else
                    {
                        Console.WriteLine($"New user has logged in!");
                        newClient = new ClientData(username, password)
                        {
                            EndPoint = clientEP,
                            TimeLoggedIn = DateTime.Now
                        };
                        clients.Add(newClient);

                        PrintClients(clients);

                        sendMessage = Encoding.UTF8.GetBytes("Welcome to the Instant-messaging server!");
                        serverSocketUDP.SendTo(sendMessage, 0, sendMessage.Length, SocketFlags.None, clientEP);
                    }


                    while (newClient.Status == Status.Idle)
                    {
                        Console.WriteLine("Waiting for channel...");

                        List<string> channelOptions = new List<string>(chatChannels);   //temp lista za biranje kanala

                        // Salje se izbor kanala
                        sendMessage = Encoding.UTF8.GetBytes("Choose a channel you would like to use:\n" + string.Join("\n", channelOptions.Select((c, i) => $"{i + 1}.{c}")));
                        serverSocketUDP.SendTo(sendMessage, 0, sendMessage.Length, SocketFlags.None, clientEP);

                        // Klijentov odgovor
                        numBytes = serverSocketUDP.ReceiveFrom(buffer, ref clientEP);
                        string chosen = Encoding.UTF8.GetString(buffer, 0, numBytes).Trim();

                        int index;
                        if(!int.TryParse(chosen, out index) || index < 1 || index > channelOptions.Count)
                        {
                            sendMessage = Encoding.UTF8.GetBytes("Invalid option. Please choose again.");
                            serverSocketUDP.SendTo(sendMessage, 0, sendMessage.Length, SocketFlags.None, clientEP);
                            Console.WriteLine($"User {username}:{clientEP} has entered a channel option that does not exist.");
                            continue;
                        }

                        string selectedChannel = channelOptions[index - 1];
                        var client = clients.FirstOrDefault(c => c.EndPoint.Equals(clientEP));
                        if (client != null)
                        {
                            client.ActiveOnChannel = selectedChannel;
                            client.Status = Status.Online;
                        }

                        sendMessage = Encoding.UTF8.GetBytes($"Successfully joined {selectedChannel.ToUpper()} channel! Start chatting now!");
                        serverSocketUDP.SendTo(sendMessage, 0, sendMessage.Length, SocketFlags.None, clientEP);
                        Console.WriteLine($"User {username}:{password}:{clientEP} has joined {selectedChannel.ToUpper()} channel!");
                    }

                    Console.WriteLine("CURRENT SERVER STATE:");
                    PrintClients(clients);
                }
                catch (SocketException e)
                { 
                    Console.WriteLine($"Socket error: {e.Message}");
                }
            }

           //serverSocketUDP.Close();

            #endregion
        }

        public static void StartClient(int numClients)
        {
            for(int i = 0; i < numClients; i++)
            {
                string clientPath = @"C:\Users\Bojana\Documents\Fakultet\III_godina\5.semestar\Primena racunarskih mreza\Projekat\PRMuIS_projekat\Client\bin\Debug\Client.exe";
                Process clientProcess = new Process();
                clientProcess.StartInfo.FileName = clientPath;
                clientProcess.StartInfo.Arguments = $"{i + 1}";
                clientProcess.Start();
                Console.WriteLine($"Client #{i + 1} started.");
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

        static List<Socket> clientSockets;
        public static void TCPConnectionSetup()
        {
            Socket serverSocketTCP = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 8001);
            serverSocketTCP.Bind(serverEP);
            serverSocketTCP.Listen(100);

            clientSockets = new List<Socket>();

            byte[] buffer = new byte[1024];
            try
            {
                Console.WriteLine("TCP socket listening on " + serverEP);
                while (true)
                {
                    Socket newClient = serverSocketTCP.Accept();
                    clientSockets.Add(newClient);
                    Console.WriteLine("Novi client se povezao: " + newClient.RemoteEndPoint);
                }
            }

            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
                serverSocketTCP.Close();
            }
        }
    }
}
