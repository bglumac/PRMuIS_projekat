using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientClass;

namespace Instant_messaging_application.Classes
{
    public class InterfaceUtil
    {
        public static async void Start()
        {
            // Init
            while (true)
            {
                Console.Clear();
                Console.WriteLine("====== SERVER ======");
                Console.WriteLine("1. Add Channel");
                Console.WriteLine("2. Logs");
                Console.WriteLine("3. Messages");

                string choice = Console.ReadLine();
                try
                {
                    int select = Convert.ToInt32(choice);
                    if (select == 1)
                    {
                        AddChannel();
                    }

                    else if (select == 2)
                    {
                        Logs();
                    }

                    else if (select == 3)
                    {
                        Messages();
                    }

                    else
                    {
                        continue;
                    }
                }
                catch { continue; }
            }
        }

        public static void AddChannel()
        {
            // WriteLine("Unesite ime kanala?")
            // Dodaj kanal
            // while (nije kanal)

            bool valid = false;

            while (!valid)
            {
                Console.Clear();
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

        public static void Logs()
        {
            Console.Clear();
            Console.WriteLine("Starting logger...");

            while (true)
            {

                Queue<string> logs = Logger.GetLogs();
                if (logs.Count == 0)
                {
                    Thread.Sleep(500);
                    continue;
                }

                
                while(logs.Count > 0)
                {
                    string msg = logs.Dequeue();
                    Console.WriteLine(msg);
                }
            }

        }

        public static void Messages()
        {
            Console.Clear();
            bool valid = false;
            while (!valid) {
                Console.Clear();
                for (int i = 0; i < ChannelHandler.channels.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {ChannelHandler.channels[i].name}");
                }

                try
                {
                    int choice = Convert.ToInt32(Console.ReadLine()) - 1;
                    ReadMessages(ChannelHandler.channels[choice].getMessages());
                    valid = true;
                }

                catch
                {
                    continue;
                }
            }
            
        }

        public static void ReadMessages(List<MessageType> messages)
        {
            foreach (MessageType message in messages)
            {
                Console.WriteLine(message.GetReadable());
            }

            Console.ReadLine();
        }
    }
}
