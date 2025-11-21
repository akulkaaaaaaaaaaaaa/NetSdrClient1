using EchoTspServer;
using NUnit.Framework;
using System.Reflection;
using System.Threading.Tasks;

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
            // Act
            MethodInfo? method = typeof(Program).GetMethod("Main");

            // Assert
            Assert.IsNotNull(method, "Main method not found");
            Assert.That(method!.ReturnType, Is.EqualTo(typeof(Task)), "Main() must return Task");
        }
    }
}
