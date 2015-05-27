using System;
using System.IO;
using NUnit.Framework;
using FOG.Handlers;
using FOG.Handlers.Data;

namespace FOGService.Tests.Handlers
{
    [TestFixture]
    public class CommunicationTests
    {
        private const string Server = "https://fog.jbob.io/fog";
        private const string MAC = "1a:2b:3c:4d:5e:6f";
        private const string URL = "/service/Test.php?unit=";
        private const string PassKeyHex = "66733579595144305635386865727967316e746236395a6c6d48355a39313863";

        [SetUp]
        public void Init()
        {
            LogHandler.Output = LogHandler.Mode.Console;
            Middleware.ServerAddress = Server;
            Middleware.TestMAC = MAC;
        }

        [Test]
        public void AESDecryptResponse()
        {
            /**
            * Ensure that decrypting AES responses works correct
            */

            Middleware.TestPassKey = Transform.HexStringToByteArray(PassKeyHex);
            var response1 = Middleware.GetResponse(string.Format("{0}AESDecryptionResponse1&key={1}", URL, PassKeyHex));
            var response2 = Middleware.GetResponse(string.Format("{0}AESDecryptionResponse2&key={1}", URL, PassKeyHex));

            Assert.IsFalse(response1.Error);
            Assert.AreEqual("Foobar22!", response1.GetField("#data"));

            Assert.IsFalse(response2.Error);
            Assert.AreEqual("Foobar22!", response2.GetField("#data"));
        }

        [Test]
        public void Download()
        {
            /**
            * Ensure that downloading a file works properly
            */

            const string phrase = "Foobar22!";
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.txt");
            var success = Middleware.DownloadFile(URL + "Download", path);

            Assert.IsTrue(success);
            Assert.IsTrue(File.Exists(path));

            var text = File.ReadAllText(path).Trim();
            Assert.AreEqual(phrase, text);

            // Test a fail download

            success = Middleware.DownloadFile("/no-exist", path);

            Assert.IsFalse(success);
        }

        [Test]
        public void GetBadResponse()
        {
            /**
            * Ensure that responses error codes are handled properely
            */
            var response = Middleware.GetResponse(URL + "BadResponse");

            Assert.IsTrue(response.Error);
            Assert.AreEqual("#!er", response.ReturnCode);
        }

        [Test]
        public void GetResponse()
        {
            /**
            * Ensure that responses can be obtained and parsed
            */
            var response = Middleware.GetResponse(URL + "Response");

            Assert.IsFalse(response.Error);
            Assert.AreEqual("bar", response.GetField("#Foo"));
            Assert.IsEmpty(response.GetField("#Empty"));
            Assert.AreEqual("Special", response.GetField("#-X"));
            Assert.IsEmpty(response.GetField("#NON_EXISTENT"));
        }

        [Test]
        public void GetRawResponse()
        {
            /**
            * Ensure that a raw responses can be obtained
            */

            const string phrase = "Foobar22!";
            var response = Middleware.GetRawResponse(URL + "RawResponse");

            Assert.AreEqual(phrase, response);
        }



        [Test]
        public void ParseResponse()
        {
            /**
            * Ensure that responses can be parsed
            */

            const string msg = "#!ok\n" +
                               "#Foo=bar\n" +
                               "#Empty=\n" +
                               "#-X=Special";

            var response = Middleware.ParseResponse(msg);

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
             * Ensure that response arrays can be parsed
             */

            const string msg = "#!ok\n" +
                               "#obj0=foo\n" +
                               "#obj1=bar\n" +
                               "#obj2=22!";

            var response = Middleware.ParseResponse(msg);
            var objArray = Middleware.ParseDataArray(response, "#obj", false);

            Assert.AreEqual(3, objArray.Count);
            Assert.AreEqual("foo", objArray[0]);
            Assert.AreEqual("bar", objArray[1]);
            Assert.AreEqual("22!", objArray[2]);
        }

        [Test]
        public void Contact()
        {
            var success = Middleware.Contact("/index.php");
            Assert.IsTrue(success);

            success = Middleware.Contact("/no-exist");
            Assert.IsFalse(success);
        }

        //This test will fail for mono due to a bug
        //TODO find mono fix
        [Test]
        public void Authenticate()
        {
            Middleware.TestPassKey = null;
            var success = Middleware.Authenticate();
            Assert.IsTrue(success);

        }
    }
}