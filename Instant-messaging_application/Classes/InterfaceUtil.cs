using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Instant_messaging_application.Classes
{
    public class InterfaceUtil
    {
        public static void Start()
        {
            // Init
        }

        public static void AddChannel()
        {
            // WriteLine("Unesite ime kanala?")
            // Dodaj kanal
            // while (nije kanal)

            bool valid = false;

            while (!valid)
            {
                Console.Write("Enter a new channel name: ");
                string name = Console.ReadLine();

                if (name == null)
                {
                    Console.WriteLine("Channel name cannot be empty. Try again.");
                    continue;
                }

                var channel = new Channel(name);

                if (ChannelHandler.channels.Contains(channel))
                {
                    Console.WriteLine("Channel already exists. Try again.");
                    continue;
                }

                ChannelHandler.channels.Add(channel);
                valid = true;
            }
        }

        public static void Logs(Queue<string> logs)
        {
            // Clear Logs
            // Go through the whole fucking stack
            // If empty, wait. If not. Print!!!

            // Clear
            // while (true) {
            // if (logs.count >0)
            // thread.sleep(1000)

            Console.Clear();

            while (true)
            {
                if(logs.Count == 0)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                while(logs.Count > 0)
                {
                    string msg = logs.Dequeue();
                    Console.WriteLine(msg);
                }
            }

        }
    }
}
