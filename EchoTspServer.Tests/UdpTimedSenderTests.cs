using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using EchoTspServer;
using NUnit.Framework;

namespace EchoServerTests
{
    [TestFixture]
    public class UdpTimedSenderTests
    {
        [Test]
        public async Task StartSending_SendsUdpPackets()
        {
            using var receiver = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
            int port = ((IPEndPoint)receiver.Client.LocalEndPoint).Port;

            using var sender = new UdpTimedSender("127.0.0.1", port);
            sender.StartSending(50);

            var receiveTask = receiver.ReceiveAsync();
            var completed = await Task.WhenAny(receiveTask, Task.Delay(1000));

            sender.StopSending();

            Assert.AreEqual(receiveTask, completed, "No UDP packet was received within timeout.");
        }
    }
}
