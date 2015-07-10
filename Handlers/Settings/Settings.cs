using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace FOG.Handlers.Settings
{
    static class Settings
    {
        private const string LogName = "Settings";

        private static readonly string _file = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\settings.json";
        private static JObject _data;

        static Settings()
        {
            try
            {
                _data = JObject.Parse(File.ReadAllText(_file));
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Unable to load settings");
                Log.Error(LogName, ex);
                _data = new JObject();
            }
        }

        private static bool Save()
        {
            try
            {
                File.WriteAllText(_file, _data.ToString());
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Unable to save settings");
                Log.Error(LogName, ex);
            }

            return false;
        }

        public static string Get(string key)
        {
            var value = _data.GetValue(key);

            return (value == null) ? null : value.ToString();
        }

        public static string Set(string key, JToken value)
        {
            _data.Add(key, value);
            Save();
        }
    }
}
