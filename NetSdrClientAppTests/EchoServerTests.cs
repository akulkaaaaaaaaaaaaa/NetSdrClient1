using NUnit.Framework;
using Moq;
using EchoServer;

namespace NetSdrClientAppTests
{
    public class EchoServerTests
    {
        [Test]
        public void Stop_Should_Call_Logger()
        {
            // Arrange
            var listenerMock = new Mock<ITcpListener>();
            var loggerMock = new Mock<ILogger>();

            var server = new EchoServer.EchoServer(listenerMock.Object, loggerMock.Object);

            // Act
            server.Stop();

            // Assert
            loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.AtLeastOnce());
        }

        [Test]
        public void UdpTimedSender_StartStop_ShouldNotThrow()
        {
            using var sender = new UdpTimedSender("127.0.0.1", 5001);

            Assert.DoesNotThrow(() =>
            {
                sender.StartSending(50);
                Task.Delay(100).Wait();
                sender.StopSending();
            });
        }
    }
}
