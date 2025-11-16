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
        [Test, Timeout(2000)]

        public async Task StartSending_SendsUdpPackets()
        {
            using var receiver = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));

            // ---- Fix nullable warning: робимо Null-check ----
            var localEp = receiver.Client.LocalEndPoint as IPEndPoint;
            Assert.NotNull(localEp, "LocalEndPoint is null unexpectedly.");
            int port = localEp!.Port;

            using var sender = new UdpTimedSender("127.0.0.1", port);
            sender.StartSending(50);

            // ---- Fix nullable warning: ReceiveAsync guaranteed not null ----
            Task<UdpReceiveResult> receiveTask = receiver.ReceiveAsync();

            Task completed = await Task.WhenAny(receiveTask, Task.Delay(1000));

            sender.StopSending();

            Assert.That(completed, Is.EqualTo(receiveTask), "No UDP packet received within timeout.");
        }
    }
}
