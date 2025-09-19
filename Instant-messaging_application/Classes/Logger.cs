using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instant_messaging_application.Classes
{
    public class Logger
    {
        Queue<string> logs = new Queue<string>();
        public void Log(string message)
        {
            logs.Append(message);
        }

        public Queue<string> GetLogs() { return logs; }
    }
}
