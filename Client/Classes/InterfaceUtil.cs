using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client.Classes
{
    internal class InterfaceUtil
    {
        public static void Start()
        {
            Auth();
            ChannelSelect();
        }

        public static void Auth()
        {
            while (!AuthHandler.getLogged())
            {
                Console.Clear();
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

                ServerUtil.getUDPSocket().SendTo(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, ServerUtil.getServerEndPointUDP());

                numBytes = ServerUtil.getUDPSocket().ReceiveFrom(recievedBuffer, ref ServerUtil.getServerEndPointUDP());
                serverMessage = Encoding.UTF8.GetString(recievedBuffer, 0, numBytes);

                if (serverMessage.Contains("Start chatting now"))
                {
                    Console.WriteLine(serverMessage);
                    AuthHandler.setLogged(true);
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
                Console.Clear();
                Console.Write("Enter channel name you want to join: ");
                string channel = Console.ReadLine();

                sendBuffer = Encoding.UTF8.GetBytes(channel);
                ServerUtil.getUDPSocket().SendTo(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, ServerUtil.getServerEndPointUDP());

                numBytes = ServerUtil.getUDPSocket().ReceiveFrom(recievedBuffer, ref ServerUtil.getServerEndPointUDP());
                serverMessage = Encoding.UTF8.GetString(recievedBuffer, 0, numBytes);

                if (serverMessage.Contains("Successfully"))
                {
                    Console.WriteLine(serverMessage);
                    validChannel = true;

                    ServerUtil.getUDPSocket().Close();
                    Console.WriteLine("UDP socket closed. Switching to TCP for chatting...");   // privremeno resenja dok se ne implementira TCP da konzola ostane u radu
                    ServerUtil.ConnectTCP();
                    Console.WriteLine("Povezivanje uspesno!");
                }
                else
                {
                    Console.WriteLine(serverMessage);
                }
            }
        }
    }
}
