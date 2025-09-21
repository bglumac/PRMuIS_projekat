using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientClass
{
    [Serializable]
    public class AuthData
    {
        string username;
        int channel_idx;

        public AuthData(string username, int channel_idx)
        {
            this.username = username;
            this.channel_idx = channel_idx;
        }

        public string Username { get => username; }
        public int Channel_idx { get => channel_idx; }
    }
}
