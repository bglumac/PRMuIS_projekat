using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using ClientClass;
using static System.Net.Mime.MediaTypeNames;

namespace Instant_messaging_application.Classes
{
    public class Channel
    {
        public String name;
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
                byte[] buffer = new byte[1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    MessageType obj = new MessageType("some user", DateTime.Now, null, message);
                    bf.Serialize(ms, obj);
                    buffer = ms.ToArray();
                    Console.WriteLine(obj.GetText());
                    item.Value.Send(buffer);
                }
                
            }
        }

        public void Join(string username, Socket socket)
        {
            users.Add(username, socket);
        }
    }
}
