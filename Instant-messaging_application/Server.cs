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
        private static int expectedClients = 3;
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

            #region Logovanje klijenta na server

            Socket serverSocketUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50000);
            serverSocketUDP.Bind(serverEP);

            serverSocketUDP.ReceiveTimeout = 30000; // ako nema promene za 30 sekundi kod pokrenutog klijenta gasi se socket za prijavu


            Console.WriteLine($"Server initialized and waiting on: {serverEP}");

            StartClient(expectedClients);

            byte[] buffer = new byte[5000];         //buffer za prijem poruka od klijenta
            byte[] sendMessage;
            int numBytes;

            while (clients.Count < expectedClients)
            {
                try
                {
                    Console.WriteLine("Waiting log in....");

                    EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);

                    ClientData newClient;

                    numBytes = serverSocketUDP.ReceiveFrom(buffer, ref clientEP);
                    string message = Encoding.UTF8.GetString(buffer, 0, numBytes);

                    string[] partsOfLogin = message.Split('|');

                    if(partsOfLogin.Length != 2)
                    {
                        Console.WriteLine($"Fomrat error on {clientEP}");
                        sendMessage = Encoding.UTF8.GetBytes("The login format does not match \"username|password\" format. Try again.");
                        serverSocketUDP.SendTo(sendMessage, clientEP);
                        continue;
                    }

                    string username = partsOfLogin[0];
                    string password = partsOfLogin[1];

                    bool alrExists = clients.Any(x =>  (x.Username == username && x.Password == password) || (x.Username == username));

                    if(alrExists)
                    {
                        sendMessage = Encoding.UTF8.GetBytes($"User {username} is already logged in!");
                        serverSocketUDP.SendTo(sendMessage, clientEP);
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

                        sendMessage = Encoding.UTF8.GetBytes("Welcome to the Instant-messaging server! Start chatting now!\nChoose a chatting channel you would like to use:\n" + string.Join("\n", chatChannels.Select(c => $"- {c}")));
                        serverSocketUDP.SendTo(sendMessage, 0, sendMessage.Length, SocketFlags.None, clientEP);
                    }


                    while (newClient.Status == Status.Idle)
                    {
                        Console.WriteLine("Waiting for channel...");

                        numBytes = serverSocketUDP.ReceiveFrom(buffer, ref clientEP);
                        message = Encoding.UTF8.GetString(buffer, 0, numBytes);

                        if (chatChannels.Contains(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(message.ToLower())))
                        {
                            Console.WriteLine($"User {username}:{password}:{clientEP} has joined {message.ToUpper()} channel!");

                            var client = clients.FirstOrDefault(c => c.EndPoint.Equals(clientEP));
                            if (client != null)
                            {
                                client.ActiveOnChannel = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(message.ToLower());
                                client.Status = Status.Online;
                            }

                            sendMessage = Encoding.UTF8.GetBytes($"Successfully joined {message.ToUpper()} channel! Start chatting!");
                            serverSocketUDP.SendTo(sendMessage, sendMessage.Length, SocketFlags.None, clientEP);
                        }
                        else
                        {
                            Console.WriteLine($"WARNING: Client at {clientEP} tried to join invalid channel: \'{message.ToUpper()}\'");
                            sendMessage = Encoding.UTF8.GetBytes("Invalid channel name. Please enter a valid channel.");
                            serverSocketUDP.SendTo(sendMessage, sendMessage.Length, SocketFlags.None, clientEP);
                            continue;
                        }
                    }

                    Console.WriteLine("CURENT SEERVER STATE:");
                    PrintClients(clients);
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.TimedOut)
                    {
                        Console.WriteLine("No login received within timeout period. Moving on...");
                        break; // izlaz iz petlje, socket može da se zatvori
                    }
                    else
                    {
                        Console.WriteLine($"Socket error: {e.Message}");
                    }
                }
            }

            serverSocketUDP.Close();

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


    }
}
