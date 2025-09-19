using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Instant_messaging_application.Classes
{
    public class ServerUtil
    {
        public static Socket clientSocketUDP;
        public static EndPoint serverEndPointUDP;

        public static Socket clientSocketTCP;
        public static IPEndPoint serverEndPointTCP;
    }
}
