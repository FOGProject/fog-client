using NUnit.Framework;
using FOG.Handlers;

namespace FOGService.Tests.Communication
{
    [TestFixture]
    public class CommunicationTest
    {
        private const string Server = "https://fog.jbob.io/fog";
        private const string MAC = "1a:2b:3c:4d:5e:6f";

        [Test]
        public void Authenticate()
        {
            /**
             * Ensure that the client can authenticate still 
             */

            CommunicationHandler.ServerAddress = Server;
            CommunicationHandler.TestMAC = MAC;

            var success = CommunicationHandler.Authenticate();

            Assert.AreEqual(true, success);
        }

        [Test]
        public void ParseResponse()
        {
            /**
            * Ensure that responses are parsed correctly
            */

            CommunicationHandler.ServerAddress = Server;
            CommunicationHandler.TestMAC = MAC;

            const string msg = "#!ok\n" +
                               "#Foo=bar\n" +
                               "#Empty=\n" +
                               "#-X=Special";

            var response = CommunicationHandler.ParseResponse(msg);

            Assert.AreEqual(false, response.Error);
            Assert.AreEqual("bar", response.GetField("#Foo"));
            Assert.AreEqual(string.Empty, response.GetField("#Empty"));
            Assert.AreEqual("Special", response.GetField("#-X"));
            Assert.AreEqual(string.Empty, response.GetField("#NON_EXISTENT"));
        }



    }
}