using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientData
{
    [Serializable]
    public class Client
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsOnline { get; set; }
        public string ActiveOnChannel { get; set; }
        public Dictionary<string, List<string>> UnreadMessages { get; set; }

        public Client()
        {
        }

        public Client(string username, string password, bool isOnline)
        {
            Username = username;
            Password = password;
            IsOnline = isOnline;
        }
    }
}
