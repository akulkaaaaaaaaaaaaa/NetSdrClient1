using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EchoServer;
using Xunit;

namespace EchoTspServerTests
{
    public class EchoStreamProcessorTests
    {
        [Fact]
        public async Task EchoProcessor_EchosData()
        {
            var processor = new EchoStreamProcessor();

            byte[] inputData = Encoding.UTF8.GetBytes("unit test payload");
            using var input = new MemoryStream();
            using var output = new MemoryStream();

            // write data to input stream and rewind
            await input.WriteAsync(inputData, 0, inputData.Length);
            input.Position = 0;

            var cts = new CancellationTokenSource(5000);
            await processor.ProcessAsync(input, output, cts.Token);

            // verify output contains same data
            output.Position = 0;
            byte[] actual = new byte[inputData.Length];
            int read = await output.ReadAsync(actual, 0, actual.Length, cts.Token);

            Assert.Equal(inputData.Length, read);
            Assert.Equal(inputData, actual);
        }
    }
}
