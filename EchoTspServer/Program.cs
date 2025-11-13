using System;
using System.Threading.Tasks;

namespace EchoServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            int tcpPort = 5000;
            string host = "127.0.0.1";
            int udpPort = 60000;
            int intervalMilliseconds = 5000;

            var logger = new ConsoleLogger();
            var listener = new DefaultTcpListener(tcpPort);
            var server = new EchoServer(listener, logger);

            // Запускаємо сервер асинхронно
            _ = Task.Run(() => server.StartAsync());

            using (var sender = new UdpTimedSender(host, udpPort))
            {
                Console.WriteLine("Press any key to stop sending...");
                sender.StartSending(intervalMilliseconds);

                Console.WriteLine("Press 'q' to quit...");
                while (Console.ReadKey(intercept: true).Key != ConsoleKey.Q)
                {
                    // чекаємо на 'q'
                }

                sender.StopSending();
                server.Stop();
                Console.WriteLine("Sender stopped.");
            }

            // Невелика пауза, щоб сервер коректно зупинився
            await Task.Delay(500);
        }
    }
}
