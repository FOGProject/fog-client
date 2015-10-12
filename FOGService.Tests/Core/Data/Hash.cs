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
using FOG.Core;
using FOG.Core.Data;
using NUnit.Framework;

namespace FOGService.Tests.Core.Data
{
    [TestFixture]
    public class HashTests
    {
        [SetUp]
        public void Init()
        {
            Log.Output = Log.Mode.Console;
        }

        [Test]
        public void MD5()
        {
            // MD5 hash a known byte set
            var testBytes = Encoding.ASCII.GetBytes("TestString");
            const string expected = "5b56f40f8828701f97fa4511ddcd25fb";

            var calculatedHash = Hash.MD5(testBytes);
            StringAssert.AreEqualIgnoringCase(expected, calculatedHash);
        }

        [Test]
        public void SHA1()
        {
            // MD5 hash a known byte set
            var testBytes = Encoding.ASCII.GetBytes("TestString");
            const string expected = "d598b03bee8866ae03b54cb6912efdfef107fd6d";

            var calculatedHash = Hash.SHA1(testBytes);
            StringAssert.AreEqualIgnoringCase(expected, calculatedHash);
        }

        [Test]
        public void SHA256()
        {
            // MD5 hash a known byte set
            var testBytes = Encoding.ASCII.GetBytes("TestString");
            const string expected = "6dd79f2770a0bb38073b814a5ff000647b37be5abbde71ec9176c6ce0cb32a27";

            var calculatedHash = Hash.SHA256(testBytes);
            StringAssert.AreEqualIgnoringCase(expected, calculatedHash);
        }

        [Test]
        public void SHA384()
        {
            // MD5 hash a known byte set
            var testBytes = Encoding.ASCII.GetBytes("TestString");
            const string expected = "c0a59eced4822f065701ec5abc51531c948864ae84391ec68e80c135d2f3fe50923445e9b436dfa2afdaa7cefa8367bb";

            var calculatedHash = Hash.SHA384(testBytes);
            StringAssert.AreEqualIgnoringCase(expected, calculatedHash);
        }

        [Test]
        public void SHA512()
        {
            // MD5 hash a known byte set
            var testBytes = Encoding.ASCII.GetBytes("TestString");
            const string expected = "69dfd91314578f7f329939a7ea6be4497e6fe3909b9c8f308fe711d29d4340d90d77b7fdf359b7d0dbeed940665274f7ca514cd067895fdf59de0cf142b62336";

            var calculatedHash = Hash.SHA512(testBytes);
            StringAssert.AreEqualIgnoringCase(expected, calculatedHash);
        }
    }
}