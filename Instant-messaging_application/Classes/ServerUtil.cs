﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Instant_messaging_application.Classes
{
    public class ServerUtil
    {
        public static Socket socketUDP;
        public static EndPoint endpointUDP;

        public static Socket socketTCP;
        public static IPEndPoint endpointTCP;

        static List<Socket> clientSockets = new List<Socket>();

        public static async Task Init()
        {
            socketTCP = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            endpointTCP = new IPEndPoint(IPAddress.Any, 50001);

            socketTCP.Bind(endpointTCP);
            socketTCP.Blocking = false;
            int maxKlijenata = 100;
            socketTCP.Listen(maxKlijenata);

            Console.WriteLine($"Listening on: {endpointTCP}");

            byte[] buffer = new byte[1024];

            try
            {
                Console.WriteLine("Server is running!");
                while (true)
                {
                    List<Socket> checkRead = new List<Socket>();
                    List<Socket> checkError = new List<Socket>();

                    if (clientSockets.Count < maxKlijenata)
                    {
                        checkRead.Add(socketTCP);
                    }
                    checkError.Add(socketTCP);

                    foreach (Socket s in clientSockets)
                    {
                        checkRead.Add(s);
                        checkError.Add(s);
                    }
                    Socket.Select(checkRead, null, checkError, 1000);

                    if (checkRead.Count > 0)
                    {
                        Console.WriteLine($"Broj dogadjaja je: {checkRead.Count}");
                        foreach (Socket s in checkRead)
                        {
                            if (s == socketTCP)
                            {
                                Socket client = socketTCP.Accept();
                                client.Blocking = false;
                                clientSockets.Add(client);
                                Console.WriteLine($"Client connected: {client.RemoteEndPoint}");
                            }

                            else
                            {
                                int numByte = s.Receive(buffer);
                                if (numByte == 0)
                                {
                                    Console.WriteLine("Client disconnected!");
                                    s.Close();
                                    clientSockets.Remove(s);

                                    continue;
                                }
                                else
                                {
                                    using (MemoryStream ms = new MemoryStream(buffer, 0, numByte))
                                    {
                                        BinaryFormatter bf = new BinaryFormatter();
                                        // Skontam od koga je po socketu?
                                    }
                                }
                            }
                        }
                    }
                    checkRead.Clear();
                }

            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            foreach (Socket s in clientSockets)
            {
                s.Send(Encoding.UTF8.GetBytes("Server closed."));
                s.Close();
            }

            Console.WriteLine("Server has shut down");
            Console.ReadKey();
            socketTCP.Close();
        }
    }
}
