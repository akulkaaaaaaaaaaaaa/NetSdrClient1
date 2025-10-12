using Moq;
using NetSdrClientApp;
using NetSdrClientApp.Networking;

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
    // 1. Покриття StopIQAsync() при відсутності з’єднання
    [Test]
    public async Task StopIQAsync_NoActiveConnection_PrintsMessage()
    {
        // Arrange
        _tcpMock.Setup(tcp => tcp.Connected).Returns(false);
        using var sw = new StringWriter();
        Console.SetOut(sw);

        // Act
        await _client.StopIQAsync();

        // Assert
        string output = sw.ToString().Trim();
        Assert.That(output, Does.Contain("No active connection."));
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

    //  3. Покриття _udpClient_MessageReceived()
    [Test]
    public void UdpClient_MessageReceived_WritesSamplesToFile()
    {
        // Arrange
        string testFile = "samples.bin";
        if (File.Exists(testFile))
            File.Delete(testFile);

        var sampleData = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        // ⚠️ Підміняємо поведінку NetSdrMessageHelper, якщо можна через static mocking
        // Якщо ні — просто перевіримо факт створення файлу та вивід у консоль
        using var sw = new StringWriter();
        Console.SetOut(sw);

        // Act
        typeof(NetSdrClient)
            .GetMethod("_udpClient_MessageReceived", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.Invoke(null, new object?[] { null, sampleData });

        // Assert
        string output = sw.ToString();
        Assert.That(output, Does.Contain("Samples recieved"));
        Assert.That(File.Exists(testFile), Is.True);

        // Clean up
        File.Delete(testFile);
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
    public async Task DisconnectWithNoConnectionTest()
    {
        //act
        _client.Disconect();

        //assert
        //No exception thrown
        _tcpMock.Verify(tcp => tcp.Disconnect(), Times.Once);
    }

    [Test]
    public async Task DisconnectTest()
    {
        //Arrange 
        await ConnectAsyncTest();

        //act
        _client.Disconect();

        //assert
        //No exception thrown
        _tcpMock.Verify(tcp => tcp.Disconnect(), Times.Once);
    }

    [Test]
    public async Task StartIQNoConnectionTest()
    {

        //act
        await _client.StartIQAsync();

        //assert
        //No exception thrown
        _tcpMock.Verify(tcp => tcp.SendMessageAsync(It.IsAny<byte[]>()), Times.Never);
        _tcpMock.VerifyGet(tcp => tcp.Connected, Times.AtLeastOnce);
    }

    [Test]
    public async Task StartIQTest()
    {
        //Arrange 
        await ConnectAsyncTest();

        //act
        await _client.StartIQAsync();

        //assert
        //No exception thrown
        _updMock.Verify(udp => udp.StartListeningAsync(), Times.Once);
        Assert.That(_client.IQStarted, Is.True);
    }

    [Test]
    public async Task StopIQTest()
    {
        //Arrange 
        await ConnectAsyncTest();

        //act
        await _client.StopIQAsync();

        //assert
        //No exception thrown
        _updMock.Verify(tcp => tcp.StopListening(), Times.Once);
        Assert.That(_client.IQStarted, Is.False);
    }

    //TODO: cover the rest of the NetSdrClient code here
}
