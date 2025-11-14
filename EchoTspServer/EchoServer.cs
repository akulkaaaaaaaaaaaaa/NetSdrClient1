using System.Threading;
using System.Threading.Tasks;

namespace EchoTspServer
{
    // Основний TCP-сервер, який приймає клієнтів і делегує їх обробку IClientHandler.
    public class EchoServer
    {
        private readonly ITcpListener _listener;
        private readonly IClientHandler _clientHandler;
        private readonly ILogger _logger;
        private CancellationTokenSource _cts;

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
                    // Stop / cancel / disposed – виходимо з циклу
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
    }
}
