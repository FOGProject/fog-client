
using System.Linq;
using System.Runtime.Serialization.Formatters;

namespace FOG.Handlers
{
    public static class SanityHandler
    {
        private const string LogName = "SanityHandler";

        public static bool AreEqual(string msg, object expected, params object[] actual)
        {
            if (actual.All(expected.Equals)) return true;

            LogHandler.Error(LogName, msg);
            return false;
        }

        public static bool AreTrue(string msg, params bool[] objects)
        {
            if (!objects.Any(obj => !obj)) return true;

            LogHandler.Error(LogName, msg);
            return false;
        }

        public static bool AreFalse(string msg, params bool[] objects)
        {
            if (!objects.Any(obj => obj)) return true;

            LogHandler.Error(LogName, msg);
            return false;
        }

        public static bool AreNotEqual(string msg, object expected, params object[] actual)
        {
            if (!actual.Any(expected.Equals)) return true;

            LogHandler.Error(LogName, msg);
            return false;
        }

        public static bool AreNull(string msg, params object[] objects)
        {
            if (objects.All(obj => obj == null)) return true;

            LogHandler.Error(LogName, msg);
            return false;
        }

        public static bool AreNotNull(string msg, params object[] objects)
        {
            if (objects.All(obj => obj != null)) return true;

            LogHandler.Error(LogName, msg);
            return false;
        }

        public static bool AreEmptyOrNull(string msg, params string[] objects)
        {
            if (objects.All(obj => obj == null || obj.Equals(string.Empty))) return true;

            LogHandler.Error(LogName, msg);
            return false;
        }

        public static bool AreNotEmptyOrNull(string msg, params string[] objects)
        {
            if (!objects.Any(obj => obj == null || obj.Equals(string.Empty))) return true;

            LogHandler.Error(LogName, msg);
            return false;
        }


    }
}
