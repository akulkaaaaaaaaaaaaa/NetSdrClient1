using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class EchoClientHandler : IClientHandler
{
    public async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        using var stream = client.GetStream();
        var buffer = new byte[8192];
        int bytesRead;

        while (!token.IsCancellationRequested &&
               (bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
        {
            await stream.WriteAsync(buffer, 0, bytesRead, token);
        }
    }
}
