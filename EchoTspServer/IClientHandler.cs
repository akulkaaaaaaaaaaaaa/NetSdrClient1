using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

public interface IClientHandler
{
    Task HandleClientAsync(TcpClient client, CancellationToken token);
}
