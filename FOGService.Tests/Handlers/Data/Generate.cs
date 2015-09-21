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

using System;
using System.Security.Cryptography;
using FOG.Core;
using FOG.Core.Data;
using NUnit.Framework;

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