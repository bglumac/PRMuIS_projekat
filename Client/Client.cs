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
            Socket clientSockeUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //IPEndPoint clientEP = new IPEndPoint(IPAddress.Loopback, 50000);  
           
            EndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 50000);  

            byte[] recievedBuffer = new byte[5000];    
            int numBytes;
            byte[] sendBuffer;
            bool logged = false;
            bool validChannel = false;
            string serverMessage;
            List<string> channels = new List<string>();


            while (true)
            {
                while (!logged)
                {
                    //Podaci klijenta
                    Console.Write("Enter your username: ");
                    string username = Console.ReadLine();
                    Console.Write("Enter your password: ");
                    string password = Console.ReadLine();

                    //u ovom formatu saljemo serveru podatke
                    string clientData = username + '|' + password;
                    sendBuffer = Encoding.UTF8.GetBytes(clientData);

                    clientSockeUDP.SendTo(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, serverEP);

                    numBytes = clientSockeUDP.ReceiveFrom(recievedBuffer, ref serverEP);
                    serverMessage = Encoding.UTF8.GetString(recievedBuffer, 0, numBytes);
                    
                    if(serverMessage.Contains("Welcome"))
                    {
                        Console.WriteLine(serverMessage);
                        logged = true;
                    }
                    else
                    {
                        Console.WriteLine($"ERROR: {serverMessage}");
                    }
                }

                while (!validChannel)
                {
                    //lista kanala od servera
                    numBytes = clientSockeUDP.ReceiveFrom(recievedBuffer, ref serverEP);
                    serverMessage = Encoding.UTF8.GetString(recievedBuffer, 0, numBytes);
                    Console.WriteLine(serverMessage);

                    Console.Write("Enter channel of your choice: ");
                    string chosen = Console.ReadLine().Trim();

                    sendBuffer = Encoding.UTF8.GetBytes(chosen);
                    clientSockeUDP.SendTo(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, serverEP);

                    numBytes = clientSockeUDP.ReceiveFrom(recievedBuffer, ref serverEP);
                    serverMessage = Encoding.UTF8.GetString(recievedBuffer, 0, numBytes);

                    while(serverMessage.Contains("Add new"))
                    {
                        Console.Write("Enter new channel name: ");
                        string newChannel = Console.ReadLine().Trim();

                        sendBuffer = Encoding.UTF8.GetBytes(newChannel);
                        clientSockeUDP.SendTo(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, serverEP);

                        //poruka servera da li je dodat novi kanal ili ne
                        numBytes = clientSockeUDP.ReceiveFrom(recievedBuffer, ref serverEP);
                        serverMessage = Encoding.UTF8.GetString(recievedBuffer, 0, numBytes);

                        Console.WriteLine(serverMessage);

                        if (serverMessage.Contains("added"))
                        { 
                            break;
                        }
                    }
                    
                    //prelazimo na TCP za chat
                    if(serverMessage.Contains("Successfully joined"))
                    {
                        Console.WriteLine(serverMessage);
                        validChannel = true;
                        clientSockeUDP.Close();
                        TCPConnect();
                    }
                    else if(serverMessage.Contains("Invalid option"))
                    {
                        Console.WriteLine(serverMessage);
                        //Console.WriteLine("Please try again...");
                    }

                }

            }

            #endregion

        }

        public static void TCPConnect()
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 8001);

            Console.WriteLine("Klijent je spreman za povezivanje sa serverom...");
            clientSocket.Connect(serverEP);
            Console.WriteLine("Povezivanje uspesno!");
        }
    }
}
