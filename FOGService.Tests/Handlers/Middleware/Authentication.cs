/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2015 FOG Project
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 3
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using FOG.Handlers;
using FOG.Handlers.Data;
using FOG.Handlers.Middleware;
using NUnit.Framework;

namespace FOGService.Tests.Handlers.Middleware
{
    [TestFixture]
    public class AuthenticationTests
    {
        [SetUp]
        public void Init()
        {
            Log.Output = Log.Mode.Console;
            Configuration.ServerAddress = Server;
            Configuration.TestMAC = MAC;
        }

        private const string Server = "http://fog.jbob.io";
        private const string MAC = "1a:2b:3c:4d:5e:6f";
        private const string URL = "/service/Test.php?unit=";
        private const string PassKeyHex = "66733579595144305635386865727967316e746236395a6c6d48355a39313863";

        [Test]
        public void AESDecryptResponse()
        {
            /**
            * Ensure that decrypting AES responses works correct
            */

            Authentication.TestPassKey = Transform.HexStringToByteArray(PassKeyHex);
            var response1 =
                Communication.GetResponse($"{URL}AESDecryptionResponse1&key={PassKeyHex}");
            var response2 =
                Communication.GetResponse($"{URL}AESDecryptionResponse2&key={PassKeyHex}");

            Assert.IsFalse(response1.Error);
            Assert.IsTrue(response1.Encrypted);
            Assert.AreEqual("Foobar22!", response1.GetField("#data"));

            Assert.IsFalse(response2.Error);
            Assert.IsTrue(response2.Encrypted);
            Assert.AreEqual("Foobar22!", response2.GetField("#data"));
        }

        [Test]
        [Ignore("Ignore test due to lack of FOG CA from build server")]
        public void Authenticate()
        {
            Authentication.TestPassKey = null;
            var success = Authentication.HandShake();
            Assert.IsTrue(success);
        }
    }
}