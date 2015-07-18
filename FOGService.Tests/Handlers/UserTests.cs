using System;
using NUnit.Framework;
using FOG.Handlers;

namespace FOGService.Tests.Handlers
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
        public void IsUserLoggedIn()
        {
            Assert.IsTrue(UserHandler.IsUserLoggedIn());
        }

        [Test]
        public void GetInactivityTime()
        {
            if(Settings.OS == Settings.OSType.Windows)
                Assert.IsTrue(UserHandler.GetInactivityTime() != -1);
            else
                Assert.IsTrue(UserHandler.GetInactivityTime() == -1);
        }

        [Test]
        public void GetCurrentUser()
        {
            Assert.AreEqual(Environment.UserName, UserHandler.GetCurrentUser());
        }

        [Test]
        public void GetUsersLoggedIn()
        {
            var users = UserHandler.GetUsersLoggedIn();
            Assert.IsTrue(users.Count >= 1);
            Assert.IsTrue(users.Contains(Environment.UserName));
        }

    }
}