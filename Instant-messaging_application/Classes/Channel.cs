using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ClientClass;

namespace Instant_messaging_application.Classes
{
    public class Channel
    {
        String name;
        List<String> messages;
        Dictionary<String, int> lastRead;

        public static Dictionary<string, Socket> users = new Dictionary<string, Socket>();

        public Channel(String name)
        {
            this.name = name;
            messages = new List<String>();
            lastRead = new Dictionary<String, int>();
        }

        public void Send(string message)
        {
            foreach (var item in users)
            {
                MessageType obj = new MessageType(message);
                item.Value.Send(obj.ToBytes());
            }
        }
    }
}
