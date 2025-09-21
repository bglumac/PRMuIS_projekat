using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instant_messaging_application.Classes
{
    public class ChannelHandler
    {
        public static List<Channel> channels = new List<Channel>()
        {
            new Channel("General"),
            new Channel("Gaming"),
            new Channel("Chill"),
            new Channel("Music"),
        };

        public static Dictionary<string, Channel> UserChannelMap = new Dictionary<string, Channel>();

        public List<Channel> Channels { get { return channels; } }

        public static void JoinChannel(Channel channel, string username) {

        }

    }
}
