using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
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
                    
                    if(serverMessage.Contains("Start chatting now"))
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
                    Console.Write("Enter channel name you want to join: ");
                    string channel = Console.ReadLine();

                    sendBuffer = Encoding.UTF8.GetBytes(channel);
                    clientSockeUDP.SendTo(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, serverEP);

                    numBytes = clientSockeUDP.ReceiveFrom(recievedBuffer, ref serverEP);
                    serverMessage = Encoding.UTF8.GetString(recievedBuffer, 0, numBytes);

                    if (serverMessage.Contains("Successfully"))
                    {
                        Console.WriteLine(serverMessage);
                        validChannel = true;

                        clientSockeUDP.Close();
                        Console.WriteLine("UDP socket closed. Switching to TCP for chatting...");   // privremeno resenja dok se ne implementira TCP da konzola ostane u radu
                    }
                    else
                    {
                        Console.WriteLine(serverMessage);
                    }
                }

            }

            #endregion

        }
    }
}
