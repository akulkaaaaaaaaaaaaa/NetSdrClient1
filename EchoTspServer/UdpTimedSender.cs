using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EchoTspServer
{
    // Періодично надсилає UDP-повідомлення.
    public class UdpTimedSender : IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private readonly UdpClient _udpClient;
        private Timer? _timer;
        private ushort _counter = 0;

        public UdpTimedSender(string host, int port)
        {
            _host = host;
            _port = port;
            _udpClient = new UdpClient();
        }

        public void StartSending(int intervalMilliseconds)
        {
            if (_timer != null)
                throw new InvalidOperationException("Sender is already running.");

            _timer = new Timer(SendMessageCallback, null, 0, intervalMilliseconds);
        }

        private void SendMessageCallback(object? state)
        {
            try
            {
                var rnd = new Random();
                var samples = new byte[1024];
                rnd.NextBytes(samples);
                _counter++;

                byte[] header = { 0x04, 0x84 };
                byte[] msg = header
                    .Concat(BitConverter.GetBytes(_counter))
                    .Concat(samples)
                    .ToArray();

                var endpoint = new IPEndPoint(IPAddress.Parse(_host), _port);
                _udpClient.Send(msg, msg.Length, endpoint);
            }
            catch
            {
                // помилки нам не критичні в бекграунді
            }
        }

        public void StopSending()
        {
            _timer?.Dispose();
            _timer = null!;
        }

        public void Dispose()
        {
            StopSending();
            _udpClient.Dispose();
        }
    }
}
