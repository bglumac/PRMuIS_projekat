using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientClass;

namespace Instant_messaging_application.Classes
{
    internal class ClientHandler
    {
        public static Dictionary<string, ClientData> clients = new Dictionary<string, ClientData>();

        public static void AddClient(ClientData data)
        {
            clients.Add(data.Username, data);
        }
    }
}
