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
using System.IO;
using FOG.Core;
using FOG.Core.Middleware;
using NUnit.Framework;

namespace FOGService.Tests.Handlers.Middleware
{
    [TestFixture]
    public class CommunicationTests
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

        [Test]
        public void Contact()
        {
            var success = Communication.Contact("/index.php");
            Assert.IsTrue(success);

            success = Communication.Contact("/no-exist");
            Assert.IsFalse(success);
        }

        [Test]
        public void Download()
        {
            /**
            * Ensure that downloading a file works properly
            */

            const string phrase = "Foobar22!";
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.txt");
            var success = Communication.DownloadFile(URL + "Download", path);

            Assert.IsTrue(success);
            Assert.IsTrue(File.Exists(path));

            var text = File.ReadAllText(path).Trim();
            Assert.AreEqual(phrase, text);

            // Test a fail download

            success = Communication.DownloadFile("/no-exist", path);

            Assert.IsFalse(success);
        }

        [Test]
        public void GetRawResponse()
        {
            /**
            * Ensure that a raw responses can be obtained
            */

            const string phrase = "Foobar22!";
            var response = Communication.GetRawResponse(URL + "RawResponse");

            Assert.AreEqual(phrase, response);
        }
    }
}