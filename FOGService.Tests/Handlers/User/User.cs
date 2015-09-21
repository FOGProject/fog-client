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
using FOG.Core;
using NUnit.Framework;

namespace FOGService.Tests.Handlers.User
{
    [TestFixture]
    public class UserTests
    {
        [SetUp]
        public void Init()
        {
            Log.Output = Log.Mode.Console;
        }

        [Test]
        public void GetCurrentUser()
        {
            Assert.AreEqual(Environment.UserName, UserHandler.GetCurrentUser());
        }

        [Test]
        [Ignore("Ignore due to CI server configuration")]
        public void GetInactivityTime()
        {
            if (FOG.Core.Settings.OS == FOG.Core.Settings.OSType.Windows)
                Assert.IsTrue(UserHandler.GetInactivityTime() != -1);
            else
                Assert.IsTrue(UserHandler.GetInactivityTime() == -1);
        }

        [Test]
        public void GetUsersLoggedIn()
        {
            var users = UserHandler.GetUsersLoggedIn();
            Assert.IsTrue(users.Count >= 1);
            Assert.IsTrue(users.Contains(Environment.UserName));
        }

        [Test]
        public void IsUserLoggedIn()
        {
            Assert.IsTrue(UserHandler.IsUserLoggedIn());
        }
    }
}