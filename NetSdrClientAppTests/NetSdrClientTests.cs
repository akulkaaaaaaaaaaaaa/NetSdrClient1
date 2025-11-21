using Moq;
using NetSdrClientApp;
using NetSdrClientApp.Networking;
using System.Reflection;


namespace NetSdrClientAppTests;

public class NetSdrClientTests
{
    NetSdrClient _client;
    using Moq;
    using NetSdrClientApp;
    using NetSdrClientApp.Networking;
    using System.Reflection;


    namespace NetSdrClientAppTests;

    public class NetSdrClientTests
    {
        NetSdrClient _client;
        Mock<ITcpClient> _tcpMock;
        Mock<IUdpClient> _updMock;

        public NetSdrClientTests() { }

        [SetUp]
        public void Setup()
        {
            _tcpMock = new Mock<ITcpClient>();
            _tcpMock.Setup(tcp => tcp.Connect()).Callback(() =>
            {
                _tcpMock.Setup(tcp => tcp.Connected).Returns(true);
            });

            _tcpMock.Setup(tcp => tcp.Disconnect()).Callback(() =>
            {
                _tcpMock.Setup(tcp => tcp.Connected).Returns(false);
            });

            _tcpMock.Setup(tcp => tcp.SendMessageAsync(It.IsAny<byte[]>())).Callback<byte[]>((bytes) =>
            {
                _tcpMock.Raise(tcp => tcp.MessageReceived += null, _tcpMock.Object, bytes);
            });

            _updMock = new Mock<IUdpClient>();

            _client = new NetSdrClient(_tcpMock.Object, _updMock.Object);
        }

        [Test]
        public async Task ConnectAsyncTest()
        {
            //act
            await _client.ConnectAsync();

            //assert
            _tcpMock.Verify(tcp => tcp.Connect(), Times.Once);
            _tcpMock.Verify(tcp => tcp.SendMessageAsync(It.IsAny<byte[]>()), Times.Exactly(3));
        }

        [Test]
        public async Task ChangeFrequencyAsync_SendsTcpRequest()
        {
            // Arrange
            _tcpMock.Setup(tcp => tcp.Connected).Returns(true);
            await _client.ConnectAsync();
            long hz = 7000000;
            int channel = 2;

            // Act
            await _client.ChangeFrequencyAsync(hz, channel);

            // Assert
            _tcpMock.Verify(tcp => tcp.SendMessageAsync(It.IsAny<byte[]>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task StartIQAsync_WhenConnected_StartsListening()
        {
            // Arrange
            _tcpMock.Setup(tcp => tcp.Connected).Returns(true);
            await _client.ConnectAsync();

            // Act
            await _client.StartIQAsync();

            // Assert
            Assert.That(_client.IQStarted, Is.True);
            _updMock.Verify(udp => udp.StartListeningAsync(), Times.Once);
        }

        [Test]
        public async Task StopIQAsync_NoActiveConnection_PrintsMessage()
        {
            // Arrange
            _tcpMock.Setup(tcp => tcp.Connected).Returns(false);
            var sw = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(sw);

            try
            {
                // Act
                await _client.StopIQAsync();

                // Assert
                string output = sw.ToString().Trim();
                Assert.That(output, Does.Contain("No active connection."));
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Test]
        public async Task ChangeFrequencyAsync_WhenNotConnected_DoesNotSend()
        {
            // Arrange
            _tcpMock.Setup(tcp => tcp.Connected).Returns(false);

            // Act
            await _client.ChangeFrequencyAsync(7000000, 1);

            // Assert
            _tcpMock.Verify(tcp => tcp.SendMessageAsync(It.IsAny<byte[]>()), Times.Never);
        }
    }
            // Act
            await _client.StopIQAsync();

            // Assert
            string output = sw.ToString().Trim();
            Assert.That(output, Does.Contain("No active connection."));
        }
        finally
        {
            Console.SetOut(originalOut);      // повертаємо Console.Out назад
        }
    }

    //  2. Покриття ChangeFrequencyAsync()
    [Test]
    public async Task ChangeFrequencyAsync_SendsTcpRequest()
    {
        // Arrange
        _tcpMock.Setup(tcp => tcp.Connected).Returns(true);
        await _client.ConnectAsync();
        long hz = 7000000;
        int channel = 2;

        // Act
        await _client.ChangeFrequencyAsync(hz, channel);

        // Assert
        _tcpMock.Verify(tcp => tcp.SendMessageAsync(It.IsAny<byte[]>()), Times.AtLeastOnce);
    }
    
    //TODO: cover the rest of the NetSdrClient code here

    [Test]
    public async Task ConnectAsync_WhenAlreadyConnected_DoesNothing()
    {
        // Arrange
        _tcpMock.Setup(tcp => tcp.Connected).Returns(true);

        // Act
        await _client.ConnectAsync();

        // Assert
        _tcpMock.Verify(tcp => tcp.Connect(), Times.Never);
    }

    [Test]
    public void Disconnect_WithActiveConnection_Works()
    {
        // Arrange
        _tcpMock.Setup(tcp => tcp.Connected).Returns(true);

        // Act
        _client.Disconect();

        // Assert
        _tcpMock.Verify(tcp => tcp.Disconnect(), Times.Once);
    }

    [Test]
    public async Task ChangeFrequencyAsync_WhenNotConnected_DoesNotSend()
    {
        // Arrange
        _tcpMock.Setup(tcp => tcp.Connected).Returns(false);

        // Act
        await _client.ChangeFrequencyAsync(7000000, 1);

        // Assert
        _tcpMock.Verify(tcp => tcp.SendMessageAsync(It.IsAny<byte[]>()), Times.Never);
    }

    [Test]
    public async Task StartIQAsync_WhenConnected_StartsListening()
    {
        // Arrange
        _tcpMock.Setup(tcp => tcp.Connected).Returns(true);
        await _client.ConnectAsync();
        var previousSendCount = 0;
        _tcpMock.Invocations.Clear();

        // Act
        await _client.StartIQAsync();

        // Assert
        Assert.That(_client.IQStarted, Is.True);
        _updMock.Verify(udp => udp.StartListeningAsync(), Times.Once);
    }

    [Test]
    public async Task StopIQAsync_WhenConnected_StopsListening()
    {
        // Arrange
        _tcpMock.Setup(tcp => tcp.Connected).Returns(true);
        await _client.ConnectAsync();
        _tcpMock.Invocations.Clear();
        _updMock.Invocations.Clear();

        // Act
        await _client.StopIQAsync();

        // Assert
        Assert.That(_client.IQStarted, Is.False);
        _updMock.Verify(udp => udp.StopListening(), Times.Once);
    }

    [Test]
    public async Task ConnectAsync_WhenAlreadyConnected_DoesNothing()
    {
        // Arrange
        _tcpMock.Setup(tcp => tcp.Connected).Returns(true);

        // Act
        await _client.ConnectAsync();

        // Assert
        _tcpMock.Verify(tcp => tcp.Connect(), Times.Never);
    }

    [Test]
    public async Task SendTcpRequest_WhenNotConnected_ReturnsEmptyArray()
    {
        // Arrange
        _tcpMock.Setup(tcp => tcp.Connected).Returns(false);
        var sw = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(sw);

        try
        {
            // Act
            var result = await _client.SendTcpRequest_Internal(new byte[] { 0x01, 0x02 });

            // Assert
            Assert.That(result, Is.Empty);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Test]
    public void Disconnect_WithActiveConnection_Works()
    {
        // Arrange
        _tcpMock.Setup(tcp => tcp.Connected).Returns(true);

        // Act
        _client.Disconect();

        // Assert
        _tcpMock.Verify(tcp => tcp.Disconnect(), Times.Once);
    }
}
