using System.Text;
using NUnit.Framework;
using FOG.Handlers;
using FOG.Handlers.Data;
using System.Security;

namespace FOGService.Tests.Handlers.Data
{
    [TestFixture]
    public class DPAPITests
    {

        [SetUp]
        public void Init()
        {
            LogHandler.Output = LogHandler.Mode.Console;
        }

        [Test]
        public void RoundTrip_Protect()
        {
            // Roundtrip a message using DPAPI protection

            const string message = "The dog jumped over the fence #@//\\\\$";
            var messageBytes = Encoding.ASCII.GetBytes(message);

            var protectedBytes = DPAPI.ProtectData(messageBytes, true);
            var unProtectedBytes = DPAPI.UnProtectData(protectedBytes, true);

            Assert.AreEqual(messageBytes, unProtectedBytes);
        }
    }
}