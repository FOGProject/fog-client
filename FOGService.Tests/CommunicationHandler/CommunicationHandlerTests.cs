using NUnit.Framework;
using FOG.Handlers;

namespace FOGService.Tests.Handlers
{
    [TestFixture]
    public class CommunicationTests
    {
        private const string Server = "http://fog.jbob.io/fog";
        private const string MAC = "1a:2b:3c:4d:5e:6f";

        [SetUp]
        public void Init()
        {
            CommunicationHandler.ServerAddress = Server;
            CommunicationHandler.TestMAC = MAC;          
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

            Assert.IsFalse(response.Error);
            Assert.AreEqual("bar", response.GetField("#Foo"));
            Assert.IsEmpty(response.GetField("#Empty"));
            Assert.AreEqual("Special", response.GetField("#-X"));
            Assert.IsEmpty(response.GetField("#NON_EXISTENT"));
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
        public void Contact()
        {
            var success = CommunicationHandler.Contact("/index.php");
            Assert.IsTrue(success);
        }

        [Test]
        public void Authenticate()
        {
            var success = CommunicationHandler.Authenticate();
            Assert.IsTrue(success);
        }



    }
}