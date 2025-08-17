using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal class Client
    {
        static void Main(string[] args)
        {

            #region Konekcija na server
            Console.Title = "Client";

            Console.WriteLine("Instant-messaging application! WELCOME!");
            Socket clientSockeUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint destinationEP = new IPEndPoint(IPAddress.Loopback, 50000);
            EndPoint sourceEP = new IPEndPoint(IPAddress.Any, 0);

            byte[] recievedBuffer = new byte[65000];
            byte[] sendBuffer;
            bool logged = false;
            List<string> channels = new List<string>();

            try
            {
                while (!logged)
                {
                    //Unos podataka klijenta
                    Console.Write("Enter your username: ");
                    string username = Console.ReadLine();
                    Console.Write("Enter your password: ");
                    string password = Console.ReadLine();
                    
                    string clientData = username + '|' + password;
                    sendBuffer = Encoding.UTF8.GetBytes(clientData);

                    try
                    {
                        //slanje serveru login podatke
                        clientSockeUDP.SendTo(sendBuffer, destinationEP);

                        int numBytes = clientSockeUDP.ReceiveFrom(recievedBuffer, ref sourceEP);
                        string serverMessage = Encoding.UTF8.GetString(recievedBuffer, 0, numBytes);

                        if (serverMessage.Contains("Try again"))
                        {
                            Console.WriteLine($"Error: {serverMessage}\n");
                        }
                        else
                        {
                            Console.WriteLine(serverMessage);
                            logged = true;
                        }

                        Console.WriteLine("Choose a chat channel you would like to use:");
                        numBytes = clientSockeUDP.ReceiveFrom(recievedBuffer, ref sourceEP);
                        serverMessage = Encoding.UTF8.GetString(recievedBuffer, 0, numBytes);

                        List<string> chatChannels = serverMessage.Split(',').ToList();
                        foreach(var ch in chatChannels)
                        {
                            Console.WriteLine($"- {ch}");
                        }

                        Console.Write("Channel name: ");
                        string chosenChannel = Console.ReadLine();

                        sendBuffer = Encoding.UTF8.GetBytes(chosenChannel);
                        clientSockeUDP.SendTo(sendBuffer, destinationEP);

                    }
                    catch(SocketException ex)
                    {
                        Console.WriteLine($"An error occurred while trying to send/recieve message\nError: {ex.Message}");
                    }
                }
            }catch(Exception ex)
            {
                Console.WriteLine("An error occcurred: " + ex.Message);
            }

            #endregion

        }
    }
}
