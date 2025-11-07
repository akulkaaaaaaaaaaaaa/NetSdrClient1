using NUnit.Framework;
using NSubstitute;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using EchoServer;

namespace EchoTspServer.Tests
{
    public class EchoServerTests
    {
        [Test]
        public async Task Server_Calls_Handler_When_Client_Connects()
        {
            var handler = Substitute.For<IClientHandler>();
            var server = new EchoServer.EchoServer(5055, handler);

            var serverTask = server.StartAsync();
            using var client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 5055);

            await Task.Delay(100);
            server.Stop();

            await handler.Received(1)
                .HandleClientAsync(Arg.Any<TcpClient>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Server_Stops_Without_Exception()
        {
            var handler = Substitute.For<IClientHandler>();
            var server = new EchoServer.EchoServer(5056, handler);

            var serverTask = server.StartAsync();
            await Task.Delay(50);

            Assert.DoesNotThrow(() => server.Stop());
        }

        [Test]
        public async Task Server_Handles_Multiple_Clients()
        {
            var handler = Substitute.For<IClientHandler>();
            var server = new EchoServer.EchoServer(5057, handler);

            var serverTask = server.StartAsync();

            using var client1 = new TcpClient();
            using var client2 = new TcpClient();

            await client1.ConnectAsync("127.0.0.1", 5057);
            await client2.ConnectAsync("127.0.0.1", 5057);

            await Task.Delay(150);
            server.Stop();

            await handler.Received(2)
                .HandleClientAsync(Arg.Any<TcpClient>(), Arg.Any<CancellationToken>());
        }
    }
}
