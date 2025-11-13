using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EchoServer
{
    public class EchoServer
    {
        private readonly ITcpListener _listener;
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _cts = new();

        public EchoServer(ITcpListener listener, ILogger logger)
        {
            _listener = listener;
            _logger = logger;
        }

        public async Task StartAsync()
        {
            _listener.Start();
            _logger.Log("Server started.");

            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    _logger.Log("Client connected.");

                    _ = Task.Run(() => HandleClientAsync(client, _cts.Token));
                }
            }
            catch (ObjectDisposedException)
            {
                // listener stopped
            }
            catch (OperationCanceledException)
            {
                // normal stop
            }
            finally
            {
                _listener.Stop();
                _logger.Log("Server shutdown.");
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            using (NetworkStream stream = client.GetStream())
            {
                try
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead;

                    while (!token.IsCancellationRequested &&
                           (bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                    {
                        await stream.WriteAsync(buffer, 0, bytesRead, token);
                        _logger.Log($"Echoed {bytesRead} bytes to the client.");
                    }
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    _logger.Log($"Error: {ex.Message}");
                }
                finally
                {
                    client.Close();
                    _logger.Log("Client disconnected.");
                }
            }
        }

        public void Stop()
        {
            _logger.Log("Server stop requested.");
            _cts.Cancel();
        }
    }
}
