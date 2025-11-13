using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EchoServer
{
    // Abstraction for processing data from an input stream to an output stream.
    public interface IStreamProcessor
    {
        Task ProcessAsync(Stream input, Stream output, CancellationToken cancellationToken);
    }
}
