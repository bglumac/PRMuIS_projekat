using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Configuration;
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
        List<MessageType> messages;
        Dictionary<string, int> lastRead;

        public static Dictionary<string, Socket> users = new Dictionary<string, Socket>();

        public Channel(String name)
        {
            this.name = name;
            messages = new List<MessageType>();
            lastRead = new Dictionary<String, int>();
        }

        public void Send(MessageType message)
        {
            message.Channel = this.name;
            foreach (var item in users)

            {
                byte[] buffer = new byte[1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, message);
                    buffer = ms.ToArray();
                    item.Value.Send(buffer);
                    messages.Add(message);
                }
            }
        }

        public void Join(string username, Socket socket)
        {
            users.Add(username, socket);
        }

        public int getUnread(string username)
        {
            if (lastRead.ContainsKey(username))
            {
                return messages.Count - lastRead[username];
            }

            return messages.Count;
        }
    }
}
