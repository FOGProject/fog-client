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

using System.Text;
using NUnit.Framework;
using FOG.Handlers;
using FOG.Handlers.Data;

namespace FOGService.Tests.Handlers.Data
{
    [TestFixture]
    public class TransformTests
    {

        [SetUp]
        public void Init()
        {
            Log.Output = Log.Mode.Console;
        }

        [Test]
        public void RoundTripBase64()
        {
            // Roundtrip a message by base64 encoding it then decoding it
            const string message = "The dog jumped over the fence #@//\\\\$";

            var encoded = Transform.EncodeBase64(message);
            Assert.IsNotEmpty(encoded);
            var decoded = Transform.DecodeBase64(encoded);

            Assert.AreEqual(message, decoded);
        }

        [Test]
        public void RoundTripHexByteString()
        {
            // Roundtrip a hex string by converting it to a byte array and then back to a hex string
            const string message = "bdb2ab3c401ef23602786e9caeb28266c18cbf06de4c634291eb4a0d51e5b7bb";

            var encoded = Transform.HexStringToByteArray(message);
            Assert.IsNotEmpty(encoded);
            var decoded = Transform.ByteArrayToHexString(encoded);

            Assert.AreEqual(message, decoded);
        }

        [Test]
        public void MD5Hash()
        {
            // MD5 hash a known byte set
            var testBytes = Encoding.ASCII.GetBytes("TestString");
            const string md5 = "5B56F40F8828701F97FA4511DDCD25FB";

            var calculatedMD5 = Transform.MD5Hash(testBytes);
            StringAssert.AreEqualIgnoringCase(md5, calculatedMD5);

            // Test invalid data
            byte[] nullBytes = null;
            Assert.IsNull(Transform.MD5Hash(nullBytes));
        }
    }
}