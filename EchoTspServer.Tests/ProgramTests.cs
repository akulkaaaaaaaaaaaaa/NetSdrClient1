using EchoTspServer;
using NUnit.Framework;

namespace EchoServerTests
{
    [TestFixture]
    public class ProgramTests
    {
        [Test]
        public void Program_Class_Exists()
        {
            Assert.IsNotNull(typeof(Program));
        }

        [Test]
        public void Main_HasCorrectSignature()
        {
            var method = typeof(Program).GetMethod("Main");

            Assert.IsNotNull(method, "Main method not found");
            Assert.AreEqual(typeof(Task), method.ReturnType, "Main() must return Task");
        }
        
    }
}
