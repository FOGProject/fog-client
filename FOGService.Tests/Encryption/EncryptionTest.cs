using FOG.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FOGService.Tests.Encryption
{
    [TestClass]
    public class EncryptionTest
    {

        [TestMethod]
        public void RoundTrip_Base64()
        {
            /**
            * Roundtrip a message by base64 encoding it then decoding it
            */

            const string message = "The dog jumped over the fence #@//\\\\$";

            var encoded = EncryptionHandler.EncodeBase64(message);
            Assert.AreNotEqual(string.Empty, encoded);
            var decoded = EncryptionHandler.DecodeBase64(encoded);

            Assert.AreEqual(message, decoded);
        }

        [TestMethod]
        public void RoundTrip_HexByteString()
        {
            /**
            * Roundtrip a hex string by converting it to a byte array and then back to a hex string
            */

            const string message = "bdb2ab3c401ef23602786e9caeb28266c18cbf06de4c634291eb4a0d51e5b7bb";

            var encoded = EncryptionHandler.HexStringToByteArray(message);
            Assert.AreNotEqual(string.Empty, encoded);
            var decoded = EncryptionHandler.ByteArrayToHexString(encoded);

            Assert.AreEqual(message, decoded);
        }

        [TestMethod]
        public void GeneratePassword()
        {

            /**
            * Generate 2 random passwords, ensure they are the correct length, and that they are not equal
            */
            const int length = 64;

            var pw1 = EncryptionHandler.GeneratePassword(length);
            var pw2 = EncryptionHandler.GeneratePassword(length);

            Assert.AreEqual(length, pw1.Length);
            Assert.AreEqual(length, pw2.Length);
            Assert.AreNotEqual(pw1, pw2);
        }



    }
}
