using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instant_messaging_application.Classes
{
    public class Logger
    {
        public static Queue<string> logs = new Queue<string>();
        public static void Log(string message)
        {
            logs.Enqueue(message);
        }

        public static Queue<string> GetLogs() { return logs; }
    }
}
