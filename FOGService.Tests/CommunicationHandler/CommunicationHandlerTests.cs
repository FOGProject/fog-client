using NUnit.Framework;
using FOG.Handlers;

namespace FOGService.Tests.Communication
{
    [TestFixture]
    public class CommunicationTest
    {
        private const string Server = "https://fog.jbob.io/fog";
        private const string MAC = "1a:2b:3c:4d:5e:6f";

        [SetUp]
        public void Init()
        {
            CommunicationHandler.ServerAddress = Server;
            CommunicationHandler.TestMAC = MAC;          
        }

        [Test]
        public void ParseDataArray()
        {
            /**
             * Ensure that response arrays an be parsed
             */

            const string msg = "#!ok\n" +
                               "#obj0=foo\n" +
                               "#obj1=bar\n" +
                               "#obj2=22!";

            var response = CommunicationHandler.ParseResponse(msg);
            var objArray = CommunicationHandler.ParseDataArray(response, "#obj", false);

            Assert.AreEqual(3, objArray.Count);
            Assert.AreEqual("foo", objArray[0]);
            Assert.AreEqual("bar", objArray[1]);
            Assert.AreEqual("22!", objArray[2]);
        }

        [Test]
        public void ParseResponse()
        {
            /**
            * Ensure that responses are parsed correctly
            */

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