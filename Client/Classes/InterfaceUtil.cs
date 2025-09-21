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
using Crypto;
using static System.Net.Mime.MediaTypeNames;

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
                //enkripcija
                sendBuffer = Vizner.Encrypt(sendBuffer);

                Console.WriteLine("Sending data?");
                ServerUtil.getUDPSocket().SendTo(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, ServerUtil.getServerEndPointUDP());


                EndPoint serverRecieveEP = new IPEndPoint(IPAddress.Any, 0);
                numBytes = ServerUtil.getUDPSocket().ReceiveFrom(recievedBuffer, ref serverRecieveEP);
                byte[] recieved = recievedBuffer.Take(numBytes).ToArray();
                byte[] dekriptovan = Vizner.Decrypt(recieved);
                //numBytes = ServerUtil.getUDPSocket().ReceiveFrom(recievedBuffer, ref ServerUtil.getServerEndPointUDP());
                serverMessage = Encoding.UTF8.GetString(dekriptovan);

                if (serverMessage.Contains("Welcome to the Instant-messaging server!"))
                {
                    Console.WriteLine(serverMessage);
                    AuthHandler.setLogged(true);
                    AuthHandler.Username = username; 
                    serverRecieveEP = new IPEndPoint(IPAddress.Any, 0);
                    numBytes = ServerUtil.getUDPSocket().ReceiveFrom(recievedBuffer, ref serverRecieveEP);
                    recieved = recievedBuffer.Take(numBytes).ToArray();
                    dekriptovan = Vizner.Decrypt(recieved);
                    //numBytes = ServerUtil.getUDPSocket().ReceiveFrom(recievedBuffer, ref ServerUtil.getServerEndPointUDP());
                    serverMessage = Encoding.UTF8.GetString(dekriptovan);
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
                //enkripcija
                sendBuffer = Vizner.Encrypt(sendBuffer);
                ServerUtil.getUDPSocket().SendTo(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, ServerUtil.getServerEndPointUDP());


                EndPoint serverRecieveEP = new IPEndPoint(IPAddress.Any, 0);
                numBytes = ServerUtil.getUDPSocket().ReceiveFrom(recievedBuffer, ref serverRecieveEP);
                byte[] recieved = recievedBuffer.Take(numBytes).ToArray();
                byte[] dekriptovan = Vizner.Decrypt(recieved);
                //numBytes = ServerUtil.getUDPSocket().ReceiveFrom(recievedBuffer, ref ServerUtil.getServerEndPointUDP());
                serverMessage = Encoding.UTF8.GetString(dekriptovan);

                if (serverMessage.Contains("Successfully"))
                {
                    Console.Clear();
                    Console.WriteLine(serverMessage);
                    validChannel = true;

                    ServerUtil.getUDPSocket().Close();
                    Console.WriteLine("UDP socket closed. Switching to TCP for chatting...");   // privremeno resenje dok se ne implementira TCP da konzola ostane u radu
                    ServerUtil.ConnectTCP();
                    Console.WriteLine("Povezivanje uspesno!");
                    byte[] buffer = new byte[1024];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        AuthData auth = new AuthData(AuthHandler.Username, Convert.ToInt32(channel)-1);
                        bf.Serialize(ms, auth);
                        buffer = ms.ToArray();
                        buffer = Vizner.Encrypt(buffer);        //enkripcija 
                        ServerUtil.getTCPSocket().Send(buffer);
                    }
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
            Task.Run(() => ReceiveMessages());
            while (true)
            {

                String text = Console.ReadLine();

                byte[] buffer = new byte[1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    MessageType message = new MessageType(AuthHandler.Username, DateTime.Now, null, text);
                    bf.Serialize(ms, message);
                    buffer = ms.ToArray();
                    buffer = Vizner.Encrypt(buffer);
                    ServerUtil.getTCPSocket().Send(buffer);
                }
            }
        }

        private static async Task ReceiveMessages()
        {
            Console.WriteLine("Started listening");
            bool listening = true;

            var stream = new NetworkStream(ServerUtil.getTCPSocket());


            while (listening)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int numByte = await stream.ReadAsync(buffer, 0, buffer.Length);
                    byte[] recived = buffer.Take(numByte).ToArray();
                    Console.WriteLine($"Received {numByte} {recived}");
                    byte[] dekriptovan = Vizner.Decrypt(recived);
                    using (MemoryStream ms = new MemoryStream(dekriptovan))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        // Skontam od koga je po socketu?
                        MessageType msg = bf.Deserialize(ms) as MessageType;
                        Console.WriteLine($"[{msg.Channel}][{msg.TimeSent.ToString("HH:mm")}] {msg.Username} -> {msg.Content}");
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
            }
        }
    }
}
