using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instant_messaging_application.Classes
{
    public class Channel
    {
        String name;
        List<String> messages;
        Dictionary<String, int> lastRead;

        public Channel(String name)
        {
            this.name = name;
            messages = new List<String>();
            lastRead = new Dictionary<String, int>();
        }
    }
}
