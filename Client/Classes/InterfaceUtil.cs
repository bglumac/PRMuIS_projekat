using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientClass;

namespace Client.Classes
{
    internal class InterfaceUtil
    {
        public static void Start()
        {
            Auth();
            ChannelSelect();
            Chat();
        }

        public static void Auth()
        {
            while (!AuthHandler.getLogged())
            {
                // Console.Clear();
                byte[] sendBuffer;
                byte[] recievedBuffer = new byte[5000];
                int numBytes;
                string serverMessage;

                //Podaci klijenta
                Console.Write("Enter your username: ");
                string username = Console.ReadLine();

                Console.Write("Enter your password: ");
                string password = Console.ReadLine();

                //u ovom formatu saljemo serveru podatke
                string clientData = username + '|' + password;
                sendBuffer = Encoding.UTF8.GetBytes(clientData);


                Console.WriteLine("Sending data?");
                ServerUtil.getUDPSocket().SendTo(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, ServerUtil.getServerEndPointUDP());


                EndPoint serverRecieveEP = new IPEndPoint(IPAddress.Any, 0);
                numBytes = ServerUtil.getUDPSocket().ReceiveFrom(recievedBuffer, ref serverRecieveEP);
                //numBytes = ServerUtil.getUDPSocket().ReceiveFrom(recievedBuffer, ref ServerUtil.getServerEndPointUDP());
                serverMessage = Encoding.UTF8.GetString(recievedBuffer, 0, numBytes);

                if (serverMessage.Contains("Welcome to the Instant-messaging server!"))
                {
                    Console.WriteLine(serverMessage);
                    AuthHandler.setLogged(true);
                    serverRecieveEP = new IPEndPoint(IPAddress.Any, 0);
                    numBytes = ServerUtil.getUDPSocket().ReceiveFrom(recievedBuffer, ref serverRecieveEP);
                    //numBytes = ServerUtil.getUDPSocket().ReceiveFrom(recievedBuffer, ref ServerUtil.getServerEndPointUDP());
                    serverMessage = Encoding.UTF8.GetString(recievedBuffer, 0, numBytes);
                    Console.WriteLine(serverMessage);
                }
                else
                {
                    Console.WriteLine($"ERROR: {serverMessage}");
                }
            }
        }

        public static void ChannelSelect()
        {
            byte[] sendBuffer;
            byte[] recievedBuffer = new byte[5000];
            int numBytes;
            string serverMessage;

            bool validChannel = false;
            while (!validChannel)
            {
                //Console.Clear();

                Console.Write("Enter channel you want to join (use numbers): ");
                string channel = Console.ReadLine();
                sendBuffer = Encoding.UTF8.GetBytes(channel);
                ServerUtil.getUDPSocket().SendTo(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, ServerUtil.getServerEndPointUDP());


                EndPoint serverRecieveEP = new IPEndPoint(IPAddress.Any, 0);
                numBytes = ServerUtil.getUDPSocket().ReceiveFrom(recievedBuffer, ref serverRecieveEP);
                //numBytes = ServerUtil.getUDPSocket().ReceiveFrom(recievedBuffer, ref ServerUtil.getServerEndPointUDP());
                serverMessage = Encoding.UTF8.GetString(recievedBuffer, 0, numBytes);

                if (serverMessage.Contains("Successfully"))
                {
                    Console.Clear();
                    Console.WriteLine(serverMessage);
                    validChannel = true;

                    ServerUtil.getUDPSocket().Close();
                    Console.WriteLine("UDP socket closed. Switching to TCP for chatting...");   // privremeno resenje dok se ne implementira TCP da konzola ostane u radu
                    ServerUtil.ConnectTCP();
                    Console.WriteLine("Povezivanje uspesno!");
                }
                else
                {
                    Console.WriteLine(serverMessage);
                    continue;
                }

            }
        }

        public static void Chat()
        {
            // Task.Run(() => ReceiveMessages());
            while (true)
            {

                String text = Console.ReadLine();

                byte[] buffer = new byte[1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    MessageType message = new MessageType(text);
                    bf.Serialize(ms, message);
                    buffer = ms.ToArray();
                    ServerUtil.getTCPSocket().Send(buffer);
                }

                byte[] buffer2 = new byte[1024];
                int numByte = ServerUtil.getTCPSocket().Receive(buffer2);
                using (MemoryStream ms = new MemoryStream(buffer2, 0, numByte))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    // Skontam od koga je po socketu?
                    MessageType msg = bf.Deserialize(ms) as MessageType;
                    Console.WriteLine(msg.Content);
                }
            }
        }

        /*private static async Task ReceiveMessages()
        {
            bool listening = true;
            while (listening)
            {
                byte[] buffer = new byte[1024];
                int numByte = ServerUtil.getTCPSocket().Receive(buffer);
                using (MemoryStream ms = new MemoryStream(buffer, 0, numByte))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    // Skontam od koga je po socketu?
                    MessageType msg = bf.Deserialize(ms) as MessageType;
                    Console.WriteLine(msg.Content);
                }
            }
        }*/
    }
}
