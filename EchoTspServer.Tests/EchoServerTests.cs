using System.Threading;
using System.Threading.Tasks;
using EchoTspServer;
using NSubstitute;
using NUnit.Framework;

namespace EchoServerTests
{
    [TestFixture]
    public class EchoServerTests
    {
        [Test]
        public async Task StartAsync_CallsHandler_WhenClientConnects()
        {
            var listener = Substitute.For<ITcpListener>();
            var handler  = Substitute.For<IClientHandler>();
            var logger   = Substitute.For<ILogger>();

            var fakeClient = new System.Net.Sockets.TcpClient();
            listener.AcceptTcpClientAsync()
                .Returns(Task.FromResult(fakeClient));

            var server = new EchoServer(listener, handler, logger);

            var serverTask = server.StartAsync();
            await Task.Delay(50);
            server.Stop();
            await Task.WhenAny(serverTask, Task.Delay(500));

            await handler.Received(1)
                .HandleClientAsync(fakeClient, Arg.Any<CancellationToken>());
        }

        [Test]
        public void Stop_DoesNotThrow_WhenCalledMultipleTimes()
        {
            var listener = Substitute.For<ITcpListener>();
            var handler  = Substitute.For<IClientHandler>();
            var logger   = Substitute.For<ILogger>();

            var server = new EchoServer(listener, handler, logger);

            Assert.DoesNotThrow(() => server.Stop());
            Assert.DoesNotThrow(() => server.Stop());
        }
    }
}
