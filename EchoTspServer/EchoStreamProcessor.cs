using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EchoServer
{
    // Simple implementation that echoes input to output.
    public class EchoStreamProcessor : IStreamProcessor
    {
        private readonly int _bufferSize;

        public EchoStreamProcessor(int bufferSize = 8192)
        {
            _bufferSize = bufferSize;
        }

        public async Task ProcessAsync(Stream input, Stream output, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[_bufferSize];
            int bytesRead;

            while (!cancellationToken.IsCancellationRequested && (bytesRead = await input.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await output.WriteAsync(buffer, 0, bytesRead, cancellationToken);
            }
        }
    }
}
