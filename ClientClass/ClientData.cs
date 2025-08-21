using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ClientClass
{

    public enum Status
    {
        Idle, 
        Online,
        Offline
    }

    [Serializable]
    public class ClientData
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public Status Status { get; set; }  
        public EndPoint EndPoint { get; set; }
        public string ActiveOnChannel { get; set; }
        public DateTime TimeLoggedIn { get; set; }
        public Dictionary<string, List<string>> UnreadMessages { get; set; }

        public ClientData()
        {
        }

        public ClientData(string username, string password)
        {
            Username = username;
            Password = password;
            Status = Status.Idle;       //Kada se na serveru stvori novi korisnik autmatski dobije status IDLE
        }
    }

}
