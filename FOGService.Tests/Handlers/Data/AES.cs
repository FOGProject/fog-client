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
using NUnit.Framework;

namespace FOGService.Tests.Handlers.Data
{
    [TestFixture]
    public class AESTests
    {
        [SetUp]
        public void Init()
        {
            Log.Output = Log.Mode.Console;
        }

        [Test]
        public void GeneratePassword()
        {
            /**
            * Generate 2 random passwords, ensure they are the correct length, and that they are not equal
            */
            const int length = 64;

            var pw1 = Generate.Password(length);
            var pw2 = Generate.Password(length);

            Assert.AreEqual(length, pw1.Length);
            Assert.AreEqual(length, pw2.Length);
            Assert.AreNotEqual(pw1, pw2);
        }

        [Test]
        public void RoundTrip_Base64()
        {
            /**
            * Roundtrip a message by base64 encoding it then decoding it
            */

            const string message = "The dog jumped over the fence #@//\\\\$";

            var encoded = Transform.EncodeBase64(message);
            Assert.IsNotEmpty(encoded);
            var decoded = Transform.DecodeBase64(encoded);

            Assert.AreEqual(message, decoded);
        }

        [Test]
        public void RoundTrip_HexByteString()
        {
            /**
            * Roundtrip a hex string by converting it to a byte array and then back to a hex string
            */

            const string message = "bdb2ab3c401ef23602786e9caeb28266c18cbf06de4c634291eb4a0d51e5b7bb";

            var encoded = Transform.HexStringToByteArray(message);
            Assert.IsNotEmpty(encoded);
            var decoded = Transform.ByteArrayToHexString(encoded);

            Assert.AreEqual(message, decoded);
        }
    }
}