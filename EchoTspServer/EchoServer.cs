using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EchoServer
{
    public class EchoServer
    {
        private readonly int _port;
        private readonly IClientHandler _handler;
        private TcpListener _listener;
        private CancellationTokenSource _cts = new();

        public EchoServer(int port, IClientHandler handler)
        {
            _port = port;
            _handler = handler;
        }

        public async Task StartAsync()
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();

            while (!_cts.Token.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync();
                _ = Task.Run(() => _handler.HandleClientAsync(client, _cts.Token));
            }
        }

        public void Stop()
        {
            _cts.Cancel();
            _listener?.Stop();
        }
    }
}
