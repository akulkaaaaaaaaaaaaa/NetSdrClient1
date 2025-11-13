using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EchoTspServerTests
{
    public class EchoServerTests
    {
        [Fact]
        public async Task EchoesDataAsync()
        {
            var server = new EchoServer.EchoServer(0);
            var serverTask = Task.Run(() => server.StartAsync());

            // wait for the listener to start and provide a port
            var sw = Stopwatch.StartNew();
            while (server.ListeningPort == 0 && sw.Elapsed < TimeSpan.FromSeconds(5))
            {
                await Task.Delay(50);
            }

            Assert.True(server.ListeningPort > 0, "Server did not start and provide a listening port.");

            using var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, server.ListeningPort);
            using var stream = client.GetStream();

            byte[] msg = Encoding.UTF8.GetBytes("hello world");
            await stream.WriteAsync(msg, 0, msg.Length);

            byte[] buffer = new byte[msg.Length];
            int read = 0;
            int toRead = msg.Length;
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            while (read < toRead)
            {
                int r = await stream.ReadAsync(buffer, read, toRead - read, cts.Token);
                if (r == 0) break;
                read += r;
            }

            Assert.Equal(msg.Length, read);
            Assert.Equal(msg, buffer);

            client.Close();
            server.Stop();
            await serverTask;
        }
    }
}
