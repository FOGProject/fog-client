using System;
using System.Security.Cryptography;
using NUnit.Framework;
using FOG.Handlers;
using FOG.Handlers.Data;

namespace FOGService.Tests.Handlers.Data
{
    [TestFixture]
    public class GenerateTests
    {

        [SetUp]
        public void Init()
        {
            Log.Output = Log.Mode.Console;
        }

        [Test]
        public void Password()
        {
            // Generate a random password, ensure they are the correct length
            const int length = 64;

            var pw = Generate.Password(length);
            Assert.AreEqual(length, pw.Length);

            // Test invalid arguments
            var nullPW = Generate.Password(-1);
            Assert.IsNull(nullPW);
        }

        [Test]
        public void Random()
        {
            // Generatea2 random number, ensure they are in the correct bounds
            var rng = new RNGCryptoServiceProvider();
            const int min = -10;
            const int max = 100;

            var num1 = Generate.Random(rng, min, max);
            Assert.IsNotNull(num1);
            Assert.IsTrue(num1 >= min && num1 <= max);

            // Test invalid arguments
            Assert.Throws<ArgumentNullException>(() => Generate.Random(null, min, max));
            Assert.Throws<ArgumentException>(() => Generate.Random(rng, max, min));
        }

    }
}