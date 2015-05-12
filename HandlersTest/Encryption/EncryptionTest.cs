using FOG.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HandlersTest.Encryption
{
    [TestClass]
    public class EncryptionTest
    {

        [TestMethod]
        public void RoundTrip_Base64()
        {
            const string message = "The dog jumped over the fence #@//\\\\$";

            var encoded = EncryptionHandler.EncodeBase64(message);
            Assert.AreNotEqual(string.Empty, encoded);
            var decoded = EncryptionHandler.DecodeBase64(encoded);

            Assert.AreEqual(message, decoded);
        }

        [TestMethod]
        public void RoundTrip_HexByteString()
        {
            const string message = "bdb2ab3c401ef23602786e9caeb28266c18cbf06de4c634291eb4a0d51e5b7bb";

            var encoded = EncryptionHandler.HexStringToByteArray(message);
            Assert.AreNotEqual(string.Empty, encoded);
            var decoded = EncryptionHandler.ByteArrayToHexString(encoded);

            Assert.AreEqual(message, decoded);
        }

        [TestMethod]
        public void RoundTrip_AES()
        {
            const string message = "tratePhudrUChuQUdzU7aqktrapRastA";
            const string key = "tDtadeqbcaba7abraguchuheZ4benRdR";
            const string iv = "tHeHut2bkReWrdTA";

            var encryptedMSG = EncryptionHandler.AESEncrypt(message, key, iv);
            Assert.AreNotEqual(string.Empty, encryptedMSG);
            var decryptedMSG = EncryptionHandler.AESDecrypt(encryptedMSG, key, iv);

            Assert.AreEqual(message, decryptedMSG);
        }

        [TestMethod]
        public void GeneratePassword()
        {
            const int length = 64;

            var pw1 = EncryptionHandler.GeneratePassword(length);
            var pw2 = EncryptionHandler.GeneratePassword(length);

            Assert.AreEqual(length, pw1.Length);
            Assert.AreEqual(length, pw2.Length);
            Assert.AreNotEqual(pw1, pw2);
        }



    }
}
