using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client.Classes
{
    public class ServerUtil
    {
        public static Socket clientSocketUDP;
        public static EndPoint serverEndPointUDP;

        public static Socket clientSocketTCP;
        public static EndPoint serverEndPointTCP;
        public static void Init()
        {
            ServerUtil.clientSocketUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ServerUtil.serverEndPointUDP = new IPEndPoint(IPAddress.Loopback, 50000);

            ServerUtil.clientSocketTCP = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerUtil.serverEndPointTCP = new IPEndPoint(IPAddress.Loopback, 8001);
        }

        public static Socket getUDPSocket()
        {
            return clientSocketUDP;
        }

        public static ref EndPoint getServerEndPointUDP()
        {
            return ref serverEndPointUDP;
        }

        public static Socket getTCPSocket()
        {
            return clientSocketTCP;
        }
        public static ref EndPoint getServerEndPointTCP() {
            return ref serverEndPointTCP;
        }

        public static void ConnectTCP()
        {
            clientSocketTCP.Connect(serverEndPointTCP);
        }

    }
}
