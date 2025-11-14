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
            var serverClient = await listener.AcceptTcpClientAsync();
            await connectTask;

            var handlerTask = handler.HandleClientAsync(serverClient, CancellationToken.None);

            byte[] sendData = { 1, 2, 3, 4 };
            var clientStream = client.GetStream();
            await clientStream.WriteAsync(sendData, 0, sendData.Length);

            byte[] buffer = new byte[4];
            int read = await clientStream.ReadAsync(buffer, 0, buffer.Length);

            Assert.AreEqual(4, read);
            Assert.AreEqual(sendData[0], buffer[0]);
            Assert.AreEqual(sendData[1], buffer[1]);
            Assert.AreEqual(sendData[2], buffer[2]);
            Assert.AreEqual(sendData[3], buffer[3]);

            client.Close();
            listener.Stop();
        }
    }
}
