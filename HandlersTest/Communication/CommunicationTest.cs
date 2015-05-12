using System.Text;
using FOG.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HandlersTest.Communication
{
    [TestClass]
    public class CommunicationTest
    {

        [TestMethod]
        public void Authenticate()
        {
            var sucess = CommunicationHandler.Authenticate();

            Assert.AreEqual(true, sucess);
        }

    }
}
