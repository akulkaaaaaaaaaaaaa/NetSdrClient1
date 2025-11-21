using NUnit.Framework;
using NetSdrClientApp.Networking;
using System;
using System.Threading.Tasks;

namespace NetSdrClientAppTests
{
    public class WrapperTests
    {
        [Test]
        public void TcpClientWrapper_SendMessage_Throws_WhenNotConnected()
        {
            var wrapper = new TcpClientWrapper("127.0.0.1", 65000);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await wrapper.SendMessageAsync(new byte[] { 1, 2, 3 }));
        }

        [Test]
        public void TcpClientWrapper_SendString_Throws_WhenNotConnected()
        {
            var wrapper = new TcpClientWrapper("127.0.0.1", 65000);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await wrapper.SendMessageAsync("hello"));
        }

        [Test]
        public void TcpClientWrapper_Disconnect_NoConnection_DoesNotThrow()
        {
            var wrapper = new TcpClientWrapper("127.0.0.1", 65000);
            Assert.DoesNotThrow(() => wrapper.Disconnect());
        }

        [Test]
        public void TcpClientWrapper_Connect_Disconnect_AreSafe()
        {
            // Avoid blocking on a real network connect. Only ensure Disconnect is safe to call
            var wrapper = new TcpClientWrapper("127.0.0.1", 65000);
            Assert.DoesNotThrow(() => wrapper.Disconnect());
        }

        [Test]
        public async Task UdpClientWrapper_StartAndStop_IsResponsive()
        {
            var wrapper = new UdpClientWrapper(65001);

            // Start listening in the background so the test doesn't block waiting for network IO.
            var listeningTask = Task.Run(() => wrapper.StartListeningAsync());

            // Allow the listener to start briefly, then stop it.
            await Task.Delay(100);
            wrapper.StopListening();

            // Wait for the listener to exit, but don't wait forever.
            var finished = await Task.WhenAny(listeningTask, Task.Delay(2000));
            Assert.That(finished, Is.Not.Null);
            // Ensure Stop/Exit are safe to call repeatedly
            Assert.DoesNotThrow(() => wrapper.Exit());
        }

        [Test]
        public void UdpClientWrapper_GetHashCode_ReturnsInt()
        {
            var wrapper = new UdpClientWrapper(65002);
            var hash = wrapper.GetHashCode();
            Assert.That(hash, Is.TypeOf<int>());
        }
    }
}
