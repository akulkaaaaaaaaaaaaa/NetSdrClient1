using NUnit.Framework;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using EchoServer;
using System.Net;

namespace EchoTspServer.Tests
{
    public class EchoClientHandlerTests
    {
        [Test]
        public async Task Handler_Echoes_Data_Back()
        {
            var handler = new EchoClientHandler();

            var listener = new TcpListener(IPAddress.Loopback, 6005);
            listener.Start();

            var acceptTask = listener.AcceptTcpClientAsync();
            var client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 6005);
            var serverClient = await acceptTask;

            var clientStream = client.GetStream();
            var serverStream = serverClient.GetStream();

            byte[] message = { 10, 20, 30, 40 };
            byte[] buffer = new byte[4];

            var handlerTask = handler.HandleClientAsync(serverClient, CancellationToken.None);

            await clientStream.WriteAsync(message, 0, message.Length);
            await serverStream.ReadAsync(buffer, 0, buffer.Length);

            CollectionAssert.AreEqual(message, buffer);

            client.Close();
            listener.Stop();
        }

        [Test]
        public async Task Handler_Stops_When_Client_Disconnects()
        {
            var handler = new EchoClientHandler();

            var listener = new TcpListener(IPAddress.Loopback, 6006);
            listener.Start();
            var accept = listener.AcceptTcpClientAsync();

            var client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 6006);
            var serverClient = await accept;

            client.Close(); // simulate disconnect

            Assert.DoesNotThrowAsync(() => handler.HandleClientAsync(serverClient, CancellationToken.None));

            listener.Stop();
        }

        [Test]
        public async Task Handler_Respects_Cancellation()
        {
            var handler = new EchoClientHandler();

            var listener = new TcpListener(IPAddress.Loopback, 6007);
            listener.Start();
            var accept = listener.AcceptTcpClientAsync();

            var client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 6007);
            var serverClient = await accept;

            var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.DoesNotThrowAsync(() => handler.HandleClientAsync(serverClient, cts.Token));

            listener.Stop();
        }
    }
}
