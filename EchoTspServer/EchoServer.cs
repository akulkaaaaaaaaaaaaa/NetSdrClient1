using System;
using System.Threading;
using System.Threading.Tasks;

namespace EchoTspServer
{
    public class EchoServer : IDisposable
    {
        private readonly ITcpListener _listener;
        private readonly IClientHandler _clientHandler;
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _cts;

        public EchoServer(ITcpListener listener, IClientHandler clientHandler, ILogger logger)
        {
            _listener = listener;
            _clientHandler = clientHandler;
            _logger = logger;
            _cts = new CancellationTokenSource();
        }

        public async Task StartAsync()
        {
            _listener.Start();
            _logger.Log("Server started.");

            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    _logger.Log("Client connected.");

                    _ = Task.Run(() => _clientHandler.HandleClientAsync(client, _cts.Token));
                }
                catch
                {
                    break;
                }
            }

            _logger.Log("Server stopped.");
        }

        public void Stop()
        {
            _cts.Cancel();
            _listener.Stop();
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
