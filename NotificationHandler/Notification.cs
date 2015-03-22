/**
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2015 FOG Project
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
**/

using System;

namespace FOG
{
    /// <summary>
    /// Store neccesary notification information
    /// </summary>
    public class Notification
    {
        //Define variables
        public String Title { get; set; }
        public String Message { get; set; }
        public int Duration { get; set; }
		
        public Notification()
        {
            Title = "";
            Message = "";
            Duration = 10;
        }
		
        public Notification(String title, String message, int duration)
        {
            Title = title;
            Message = message;
            Duration = duration;
        }
    }
}
