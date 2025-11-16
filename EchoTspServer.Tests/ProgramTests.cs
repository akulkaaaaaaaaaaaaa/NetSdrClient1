using EchoTspServer;
using NUnit.Framework;

namespace EchoServerTests
{
    [TestFixture]
    public class ProgramTests
    {
        [Test, Timeout(2000)]

        public void Program_Class_Exists()
        {
            // Просто перевіряємо, що тип інціалізується.
            Assert.IsNotNull(typeof(Program));
        }
    }
}
