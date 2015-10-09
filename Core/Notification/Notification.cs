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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace FOG.Core
{
    public static class Notification
    {
        private static object locker = new object();
        private static volatile Dictionary<string, JObject> onGoingList = new Dictionary<string, JObject>();

        public static JObject ToJSON(string title, string message, string subjectID)
        {
            dynamic json = new JObject();
            json.title = title;
            json.message = message;
            json.subjectID = subjectID;
            return json;
        }

        public static void Emit(string title, string message, string subjectID = "", bool onGoing = false, bool global = true)
        {
            Emit(ToJSON(title, message, subjectID), onGoing, global);
        }

        public static void Emit(JObject data, bool onGoing = false, bool global = true)
        {
            if (data["subjectID"] == null) throw new ArgumentNullException();

            onGoingList.Remove(data["subjectID"].ToString());

            if (onGoing)
                onGoingList.Add(data["subjectID"].ToString(), data);
            if(global)
                Record(data);

            Bus.Emit(Bus.Channel.Notification, data, global);
        }

        public static JObject[] GetOnGoing()
        {
            return onGoingList.Values.ToArray();
        }

        public static void Record(JObject data)
        {
            if(data["title"] != null)
                Record(data["title"].ToString());
        }

        public static void Record(string title)
        {
            lock (locker)
            {
                //Write message to log file
                var logWriter = new StreamWriter(CalculateLogName(), true);
                logWriter.WriteLine($"{DateTime.Now.ToShortDateString()} {title}");
                logWriter.Close();
            }
        }

        private static string CalculateLogName()
        {
            var logPath = Path.Combine(Settings.Location, "logs", DateTime.Today.ToString("yy-MM-dd"));
            return logPath;
        }
    }
}