using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetSdrClientApp.Networking
{
    public sealed partial class UdpClientWrapper : IUdpClient, IDisposable
    {
        private readonly IPEndPoint _localEndPoint;
        private CancellationTokenSource? _cts;
        private UdpClient? _udpClient;
        private bool _disposed;

        public event EventHandler<byte[]>? MessageReceived;

        public UdpClientWrapper(int port)
        {
            _localEndPoint = new IPEndPoint(IPAddress.Any, port);
        }

        public async Task StartListeningAsync()
        {
            ThrowIfDisposed();

            _cts = new CancellationTokenSource();
            Console.WriteLine("Start listening for UDP messages...");

            try
            {
                _udpClient = new UdpClient(_localEndPoint);

                while (!_cts.Token.IsCancellationRequested)
                {
                    UdpReceiveResult result = await _udpClient.ReceiveAsync(_cts.Token);
                    MessageReceived?.Invoke(this, result.Buffer);

                    Console.WriteLine($"Received from {result.RemoteEndPoint}");
                }
            }
            catch (OperationCanceledException)
            {
                // expected on Stop
            }
            catch (ObjectDisposedException)
            {
                // disposed concurrently — ignore
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving message: {ex.Message}");
            }
        }

        private void StopInternal()
        {
            try
            {
                _cts?.Cancel();
                _udpClient?.Close();
                _cts?.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // already cleaned up in parallel shutdown
            }
            finally
            {
                _udpClient = null;
                _cts = null;
                Console.WriteLine("Stopped listening for UDP messages.");
            }
        }

        public void StopListening() => StopInternal();

        public void Exit() => StopInternal();

        public override int GetHashCode()
        {
            var payload = $"{nameof(UdpClientWrapper)}|{_localEndPoint.Address}|{_localEndPoint.Port}";
            var data = Encoding.UTF8.GetBytes(payload);

            // Prefer static SHA256.HashData - compliant with Sonar recommendation
            var hash = SHA256.HashData(data);
            return BitConverter.ToInt32(hash, 0);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UdpClientWrapper));
        }
    }

    public sealed partial class UdpClientWrapper
    {
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;

            if (disposing)
            {
                // release managed resources
                StopInternal();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
