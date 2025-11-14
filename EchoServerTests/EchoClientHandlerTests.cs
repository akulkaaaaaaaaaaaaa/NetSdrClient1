using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using EchoTspServer;
using NUnit.Framework;

namespace EchoServerTests
{
    [TestFixture]
    public class EchoClientHandlerTests
    {
        [Test]
        public async Task HandleClientAsync_EchoesBackData()
        {
            var handler = new EchoClientHandler();

            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;

            var client = new TcpClient();

            var connectTask = client.ConnectAsync("127.0.0.1", port);
            var acceptTask  = listener.AcceptTcpClientAsync();

            await Task.WhenAll(connectTask, acceptTask);

            var serverClient = acceptTask.Result;

            var handlerTask = handler.HandleClientAsync(serverClient, CancellationToken.None);

            byte[] sendData = { 1, 2, 3, 4 };
            var clientStream = client.GetStream();
            await clientStream.WriteAsync(sendData, 0, sendData.Length);

            byte[] buffer = new byte[4];
            int read = await clientStream.ReadAsync(buffer, 0, buffer.Length);

            Assert.That(read, Is.EqualTo(4));
            Assert.That(buffer[0], Is.EqualTo(sendData[0]));
            Assert.That(buffer[1], Is.EqualTo(sendData[1]));
            Assert.That(buffer[2], Is.EqualTo(sendData[2]));
            Assert.That(buffer[3], Is.EqualTo(sendData[3]));

            client.Close();
            serverClient.Close();
            listener.Stop();
        }
    }
}
