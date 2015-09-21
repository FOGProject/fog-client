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

using System.IO;
using FOG.Core;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace FOGService.Tests.Handlers.Settings
{
    [TestFixture]
    public class SettingsTests
    {
        [SetUp]
        public void Init()
        {
            WriteSettings();
            Log.Output = Log.Mode.Console;
            FOG.Core.Settings.SetPath("settings.json");
        }

        [TearDown]
        public void Dispose()
        {
            File.Delete("settings.json");
        }

        private const string Https = "0";
        private const string Tray = "1";
        private const string Server = "fog.jbob.io";
        private const string Webroot = "";
        private const string Version = "1.9.2";
        private const string Company = "FOG";
        private const string Rootlog = "0";

        private void WriteSettings()
        {
            var settings = new JObject
            {
                {"HTTPS", Https},
                {"Tray", Tray},
                {"Server", Server},
                {"WebRoot", Webroot},
                {"Version", Version},
                {"Company", Company},
                {"RootLog", Rootlog}
            };

            File.WriteAllText("settings.json", settings.ToString());
        }

        [Test]
        public void BadGet()
        {
            Assert.IsNullOrEmpty(FOG.Core.Settings.Get("NO_EXIST"));
            Assert.IsNullOrEmpty(FOG.Core.Settings.Get("https"));
        }

        [Test]
        public void Get()
        {
            Assert.AreEqual(Https, FOG.Core.Settings.Get("HTTPS"));
            Assert.AreEqual(Tray, FOG.Core.Settings.Get("Tray"));
            Assert.AreEqual(Server, FOG.Core.Settings.Get("Server"));
            Assert.AreEqual(Webroot, FOG.Core.Settings.Get("WebRoot"));
            Assert.AreEqual(Version, FOG.Core.Settings.Get("Version"));
            Assert.AreEqual(Company, FOG.Core.Settings.Get("Company"));
            Assert.AreEqual(Rootlog, FOG.Core.Settings.Get("RootLog"));
        }

        [Test]
        public void Set()
        {
            FOG.Core.Settings.Set("foo", "bar");
            Assert.AreEqual("bar", FOG.Core.Settings.Get("foo"));
        }
    }
}