using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using ClientClass;
using Crypto;
using Microsoft.Win32;

namespace Instant_messaging_application.Classes
{
    public class ServerUtil
    {
        public static Socket socketUDP;
        public static EndPoint endpointUDP;

        public static Socket socketTCP;
        public static IPEndPoint endpointTCP;

        static List<Socket> clientSockets = new List<Socket>();
        static Dictionary<Socket, bool> authList = new Dictionary<Socket, bool>();

        public static async Task Init()
        {
            socketTCP = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            endpointTCP = new IPEndPoint(IPAddress.Any, 8001);

            socketTCP.Bind(endpointTCP);
            socketTCP.Blocking = false;
            int maxKlijenata = 100;
            socketTCP.Listen(maxKlijenata);

            // Console.WriteLine($"Listening on: {endpointTCP}");
            Logger.Log($"Listening on: {endpointTCP}");

            byte[] buffer = new byte[1024];

            try
            {
                // Console.WriteLine("Server is running!");
                Logger.Log("Server is running!");
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
                        // Console.WriteLine("Dogadjaji " + checkRead.Count);
                        Logger.Log("Dogadjaji " + checkRead.Count);
                        foreach (Socket s in checkRead)
                        {
                            if (s == socketTCP)
                            {
                                Socket client = socketTCP.Accept();
                                client.Blocking = false;

                                authList.Add(client, false);

                                clientSockets.Add(client);
                                // Console.WriteLine($"Client connected: {client.RemoteEndPoint}");
                                Logger.Log($"Client connected: {client.RemoteEndPoint}");
                            }

                            else
                            {
                                try
                                {
                                    int numByte = s.Receive(buffer);
                                    byte[] recieved = buffer.Take(numByte).ToArray();
                                    byte[] dekriptovan = Vizner.Decrypt(recieved);

                                    if (!authList[s])
                                    {

                                        // Console.WriteLine("Authenticating...");
                                        Logger.Log("Authenticating...");

                                        using (MemoryStream ms = new MemoryStream(dekriptovan, 0, numByte))
                                        {
                                            BinaryFormatter bf = new BinaryFormatter();
                                            // Skontam od koga je po socketu?
                                            AuthData data = bf.Deserialize(ms) as AuthData;
                                            authList[s] = true; //sad ispise sve ali pukne na 93
                                            // Console.WriteLine(data.Username + " connected to " + ChannelHandler.channels[data.Channel_idx].name);
                                            Logger.Log(data.Username + " connected to " + ChannelHandler.channels[data.Channel_idx].name);
                                            ChannelHandler.JoinChannel(ChannelHandler.channels[data.Channel_idx], data.Username, s);
                                        }
                                    }


                                    else if (numByte == 0)
                                    { 
                                        GracefulDisconnect(s);
                                        continue;
                                    }

                                    else
                                    {
                                        recieved = buffer.Take(numByte).ToArray();
                                        dekriptovan = Vizner.Decrypt(recieved);
                                        using (MemoryStream ms = new MemoryStream(dekriptovan, 0, numByte))
                                        {
                                            BinaryFormatter bf = new BinaryFormatter();
                                            MessageType msg = bf.Deserialize(ms) as MessageType;
                                            ChannelHandler.UserChannelMap[msg.Username].Send(msg);
                                        } 
                                    }
                                }

                                catch (Exception ex)
                                {
                                    GracefulDisconnect(s);
                                   
                                    continue;           
                                }
                                
                            }
                        }
                    }
                    checkRead.Clear();
                }

            }

            catch (Exception ex)
            {
                // Console.WriteLine(ex.Message);
                Logger.Log(ex.Message);
            }

            foreach (Socket s in clientSockets)
            {
                s.Send(Encoding.UTF8.GetBytes("Server closed."));
                s.Close();
            }

            // Console.WriteLine("Server has shut down");
            Logger.Log("Server has shut down");
            Console.ReadKey();
            socketTCP.Close();
        }

        public static void GracefulDisconnect(Socket s)
        {
            s.Close();
            authList.Remove(s);
            clientSockets.Remove(s);
            
            foreach (var channel in ChannelHandler.channels)
            {
                string toRemove = null;
                foreach (var user in channel.users)
                {
                    if (user.Value == s)
                    {
                        toRemove = user.Key;
                    }
                }
                if (toRemove == null) continue;

                channel.users[toRemove] = null;

                ClientHandler.clients[toRemove].Status = Status.Offline;
            }

            // Console.WriteLine("Client disconnected!");
            Logger.Log("Client disconnected!");

        }
    }
    }

