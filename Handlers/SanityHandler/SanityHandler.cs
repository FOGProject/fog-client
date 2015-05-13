
using System.Linq;
using System.Runtime.Serialization.Formatters;

namespace FOG.Handlers
{
    public static class SanityHandler
    {
        private const string LogName = "SanityHandler";

        /// <summary>
        /// Check if any of the object(s) are null
        /// </summary>
        /// <param name="objects">The object(s) to check</param>
        /// <returns></returns>
        public static bool AnyNull(params object[] objects)
        {
            return objects.Any(obj => obj == null);
        }

        /// <summary>
        /// Check if any of the object(s) are null, if so log an error
        /// </summary>
        /// <param name="msg">The error message to log</param>
        /// <param name="objects">The object(s) to check</param>
        /// <returns></returns>
        public static bool AnyNull(string msg, params object[] objects)
        {
            if (!AnyNull(objects)) return false;

            LogHandler.Error(LogName, msg);
            return true;
        }

        /// <summary>
        /// Check if any of the string(s) are null or empty
        /// </summary>
        /// <param name="strings">The string(s) to check</param>
        /// <returns></returns>
        public static bool AnyNullOrEmpty(params string[] strings)
        {
            return strings.Any(string.IsNullOrEmpty);
        }

        /// <summary>
        /// Check if any of the string(s) are null or empty, if so log an error
        /// </summary>
        /// <param name="msg">The error message to log</param>
        /// <param name="strings">The string(s) to check</param>
        /// <returns></returns>
        public static bool AnyNullOrEmpty(string msg, params string[] strings)
        {
            if (!AnyNullOrEmpty(strings)) return false;

            LogHandler.Error(LogName, msg);
            return true;
        }

        /// <summary>
        /// Check if any of the object(s) are equal to expected
        /// </summary>
        /// <param name="expected">The object to compare to</param>
        /// <param name="objects">The object(s) to check</param>
        /// <returns></returns>
        public static bool AnyEqual(object expected, params object[] objects)
        {
            return objects.Any(expected.Equals);
        }

        /// <summary>
        /// Check if any of the object(s) are equal to expected, if so log an error
        /// </summary>
        /// <param name="msg">The error to log</param>
        /// <param name="expected">The object to compare to</param>
        /// <param name="objects">The object(s) to check</param>
        /// <returns></returns>
        public static bool AnyEqual(string msg, object expected, params object[] objects)
        {
            if (!AnyEqual(objects)) return false;

            LogHandler.Error(LogName, msg);
            return true;
        }

        /// <summary>
        /// Check if any of the object(s) are not equal to expected
        /// </summary>
        /// <param name="expected">The object to compare to</param>
        /// <param name="objects">The object(s) to check</param>
        /// <returns></returns>
        public static bool AnyNotEqual(object expected, params object[] objects)
        {
            return objects.Any(obj => !expected.Equals(obj));
        }

        /// <summary>
        /// Check if any of the object(s) are not equal to expected, if so log an error
        /// </summary>
        /// <param name="msg">The error to log</param>
        /// <param name="expected">The object to compare to</param>
        /// <param name="objects">The object(s) to check</param>
        /// <returns></returns>
        public static bool AnyNotEqual(string msg, object expected, params object[] objects)
        {
            if (!AnyNotEqual(objects)) return false;

            LogHandler.Error(LogName, msg);
            return true;
        }

        /// <summary>
        /// Check if any of the object(s) are duplicates
        /// </summary>
        /// <param name="objects">The object(s) to check</param>
        /// <returns></returns>
        public static bool AnyDuplicates(params object[] objects)
        {
            return objects.Distinct().Count() == objects.Length;
        }

        /// <summary>
        /// Check if any of the object(s) are duplicates, if so log an error
        /// </summary>
        /// <param name="msg">The error to log</param>
        /// <param name="objects">The object(s) to check</param>
        /// <returns></returns>
        public static bool AnyDuplicates(string msg, params object[] objects)
        {
            if (!AnyDuplicates(objects)) return false;

            LogHandler.Error(LogName, msg);
            return true;
        }

    }
}
