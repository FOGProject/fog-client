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
using System.Reflection;
using NUnit.Framework;
using FOG.Handlers;
using Newtonsoft.Json.Linq;

namespace FOGService.Tests.Handlers.Settings
{
    [TestFixture]
    public class SettingsTests
    {
        private const string Https = "0";
        private const string Tray = "1";
        private const string Server = "fog.jbob.io";
        private const string Webroot = "";
        private const string Version = "1.9.2";
        private const string Company = "FOG";
        private const string Rootlog = "0";

        [SetUp]
        public void Init()
        {
            Log.Output = Log.Mode.Console;
            WriteSettings();
            FOG.Handlers.Settings.Reload();
        }

        [TearDown]
        public void Dispose()
        {
            File.Delete(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.json"));
        }

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

            File.WriteAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.json"), settings.ToString());
        }

        [Test]
        public void Get()
        {
            Assert.AreEqual(Https, FOG.Handlers.Settings.Get("HTTPS"));
            Assert.AreEqual(Tray, FOG.Handlers.Settings.Get("Tray"));
            Assert.AreEqual(Server, FOG.Handlers.Settings.Get("Server"));
            Assert.AreEqual(Webroot, FOG.Handlers.Settings.Get("WebRoot"));
            Assert.AreEqual(Version, FOG.Handlers.Settings.Get("Version"));
            Assert.AreEqual(Company, FOG.Handlers.Settings.Get("Company"));
            Assert.AreEqual(Rootlog, FOG.Handlers.Settings.Get("RootLog"));
        }

        [Test]
        public void BadGet()
        {
            Assert.IsNotNullOrEmpty(FOG.Handlers.Settings.Get("NO_EXIST"));
            Assert.IsNotNullOrEmpty(FOG.Handlers.Settings.Get("https"));
        }

        [Test]
        public void Set()
        {
            FOG.Handlers.Settings.Set("foo", "bar");
            Assert.AreEqual("bar", FOG.Handlers.Settings.Get("foo"));
        }
    }
}