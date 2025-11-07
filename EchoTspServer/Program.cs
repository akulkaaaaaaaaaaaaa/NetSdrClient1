using System;
using System.Threading.Tasks;

namespace EchoServer
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var handler = new EchoClientHandler();
            var server = new EchoServer(5000, handler);

            _ = Task.Run(() => server.StartAsync());

            Console.WriteLine("Press any key to start sending UDP messages...");
            Console.ReadKey(true);

            using var sender = new UdpTimedSender("127.0.0.1", 60000);
            sender.StartSending(3000);

            Console.WriteLine("Press 'Q' to stop...");
            while (Console.ReadKey(true).Key != ConsoleKey.Q) { }

            sender.StopSending();
            server.Stop();
        }
    }
}
