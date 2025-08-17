using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using ClientData;

namespace Instant_messaging_application
{
    internal class Server
    {

        public static List<Client> clients = new List<Client>();
        public static List<string> chatChannels = new List<string>()
        {
            "General",
            "Gaming",
            "ChillZone",
            "Music"
        };

        static void Main(string[] args)
        {
            Console.WriteLine("Instant-messaging application server!");

            #region Logovanje klijenta na server

            Socket serverSocketUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50000);
            serverSocketUDP.Bind(serverEP);
            EndPoint sourceEP = new IPEndPoint(IPAddress.Any, 0);

            Console.WriteLine($"Server initialized and waiting on: {serverEP}");

            byte[] recievedBuffer = new byte[65000];
            byte[] sendMessage;

            try
            {
                while (true)
                {
                    try
                    {
                        int numBytes = serverSocketUDP.ReceiveFrom(recievedBuffer, ref sourceEP);
                        string message = Encoding.UTF8.GetString(recievedBuffer, 0, numBytes);

                        //format prijave korisnika je "username|password"
                        string[] partsOfLogin = message.Split('|');

                        if(partsOfLogin.Length != 2)
                        {
                            Console.WriteLine($"Fomrat error on {sourceEP}");
                            sendMessage = Encoding.UTF8.GetBytes("The login format does not match \"username|password\" format. Try again.");

                            serverSocketUDP.SendTo(sendMessage, sourceEP);
                        }
                        
                        string username = partsOfLogin[0];
                        string password = partsOfLogin[1];


                        //work in progress
                        //Client existingClient = clients.FirstOrDefault(c => c.Username == username && c.Password == password);

                        //if(existingClient == null)
                        //{
                        //    Client newClient = new Client(username, password, true);
                        //    clients.Add(newClient);
                        //    Console.WriteLine($"Added new client: {username}:{password} at {sourceEP}");
                        //}
                        //else
                        //{
                        //    Console.WriteLine("The client is already logged in!")
                        //}

                        Console.WriteLine($"Successful login from: {sourceEP} user. ====> Username: {username}; Password: {password}");
                        sendMessage = Encoding.UTF8.GetBytes($"Successful login! Welcome {username}!");
                        serverSocketUDP.SendTo(sendMessage, sourceEP);

                        Console.WriteLine("Sending available channels to choose from...");

                        //slanje kanala koji su opcija za biranje
                        string channelsStr = string.Join(",", chatChannels);
                        sendMessage = Encoding.UTF8.GetBytes(channelsStr);
                        serverSocketUDP.SendTo(sendMessage, sourceEP);


                        numBytes = serverSocketUDP.ReceiveFrom(recievedBuffer, ref sourceEP);
                        message = Encoding.UTF8.GetString(recievedBuffer, 0, numBytes);
                        Console.WriteLine($"User {username}:{password} has chosen the {message} channel.");

                        

                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine($"An error occured while trying to log in to server!\nError message: {ex.Message}");
                    }
                }
            }
            catch(SocketException ex)
            {
                Console.WriteLine($"An error occurred:\n{ex.Message}");
            }

            #endregion
        }
    }
}
