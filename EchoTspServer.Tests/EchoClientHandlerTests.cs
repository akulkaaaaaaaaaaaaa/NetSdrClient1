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
        public async Task HandleClientAsync_EchoesBackData_Fast()
        {
            using var s1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            using var s2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Створюємо "віртуальний конект" між сокетами
            var ep = new IPEndPoint(IPAddress.Loopback, 0);
            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(ep);
            listener.Listen(1);

            var port = ((IPEndPoint)listener.LocalEndPoint).Port;

            var connectTask = s1.ConnectAsync(IPAddress.Loopback, port);
            var acceptTask  = listener.AcceptAsync();

            await Task.WhenAll(connectTask, acceptTask);

            var serverClient = acceptTask.Result;
            var handler = new EchoClientHandler();

            var handlerTask = handler.HandleClientAsync(new TcpClient { Client = serverClient }, CancellationToken.None);

            byte[] sendData = { 1, 2, 3, 4 };
            await s1.SendAsync(sendData);

            byte[] buffer = new byte[4];
            int read = await s1.ReceiveAsync(buffer);

            Assert.AreEqual(4, read);
            Assert.That(buffer, Is.EqualTo(sendData));

            listener.Close();
        }
    }
}
