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
using FOG.Handlers.Middleware;
using NUnit.Framework;

namespace FOGService.Tests.Handlers.Middleware
{
    [TestFixture]
    public class ResponseTests
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
        public void GetBadResponse()
        {
            /**
            * Ensure that responses error codes are handled properely
            */
            var response = Communication.GetResponse(URL + "BadResponse");

            Assert.IsTrue(response.Error);
            Assert.AreEqual("#!er", response.ReturnCode);
        }

        [Test]
        public void GetResponse()
        {
            /**
            * Ensure that responses can be obtained and parsed
            */
            var response = Communication.GetResponse(URL + "Response");

            Assert.IsFalse(response.Error);
            Assert.AreEqual("bar", response.GetField("#Foo"));
            Assert.IsEmpty(response.GetField("#Empty"));
            Assert.AreEqual("Special", response.GetField("#-X"));
            Assert.IsNullOrEmpty(response.GetField("#NON_EXISTENT"));
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

            var response = new Response(msg);
            var objArray = response.GetList("#obj", false);

            Assert.AreEqual(3, objArray.Count);
            Assert.AreEqual("foo", objArray[0]);
            Assert.AreEqual("bar", objArray[1]);
            Assert.AreEqual("22!", objArray[2]);
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

            var response = new Response(msg);

            Assert.IsFalse(response.Error);
            Assert.AreEqual("bar", response.GetField("#Foo"));
            Assert.IsEmpty(response.GetField("#Empty"));
            Assert.AreEqual("Special", response.GetField("#-X"));
            Assert.IsNullOrEmpty(response.GetField("#NON_EXISTENT"));
        }
    }
}