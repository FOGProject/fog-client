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

using System.Collections.Generic;

namespace FOG.Handlers.NotificationHandler
{
    /// <summary>
    ///     Handle all notifications
    /// </summary>
    public static class NotificationHandler
    {
        private static bool _initialized = Initialize();
        //Define variable
        public static List<Notification> Notifications { get; set; }
        public static string Company { get; private set; }

        private static bool Initialize()
        {
            Notifications = new List<Notification>();
            Company = "FOG";

            return true;
        }
    }
}