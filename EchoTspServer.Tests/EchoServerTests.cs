using System.Threading;
using System.Threading.Tasks;
using EchoTspServer;
using NSubstitute;
using NUnit.Framework;
using System.Net.Sockets;

namespace EchoServerTests
{
    [TestFixture]
    public class EchoServerTests
    {
        [Test, Timeout(2000)]
        public async Task StartAsync_CallsHandler_WhenClientConnects()
        {
            var listener = Substitute.For<ITcpListener>();
            var handler = Substitute.For<IClientHandler>();
            var logger = Substitute.For<ILogger>();

            var fakeClient = new TcpClient();

            // 1-й виклик → повертає клієнта
            // 2-й виклик → завдання, яке ніколи не завершується (імітація очікування наступного клієнта)
            listener.AcceptTcpClientAsync()
                .Returns(
                    Task.FromResult(fakeClient),
                    Task.Delay(Timeout.Infinite).ContinueWith(_ => new TcpClient())
                );

            var server = new EchoServer(listener, handler, logger);

            var serverTask = server.StartAsync();

            // Даємо серверу стартанути
            await Task.Delay(50);

            // Зупиняємо
            server.Stop();

            // Чекаємо завершення або fallback по таймауту
            await Task.WhenAny(serverTask, Task.Delay(500));

            // Перевіряємо що handler був викликаний 1 раз
            await handler.Received(1).HandleClientAsync(fakeClient, Arg.Any<CancellationToken>());
        }

        [Test, Timeout(2000)]

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
