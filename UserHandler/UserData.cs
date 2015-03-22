
using System;

namespace FOG
{
    /// <summary>
    /// Hold information about a specific user account
    /// </summary>
    public class UserData
    {
        public String Name { get; private set; }
        public String SID  { get; private set; }

        public UserData(String name, String sid)
        {
            Name = name;
            SID = sid;
        }
    }
}
