/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2023 FOG Project
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

namespace FOG.Modules.DataContracts
{
    public class Snapin
    {
        public int JobTaskID = -1;
        public string Name = "";
        public string JobCreation = "-1";
        public string Action = "";

        public string RunWith = "";
        public string RunWithArgs = "";

        public string FileName = "";
        public string Args = "";
        public string Hash = "";

        public string Url = "";
        public bool Pack = false;
        public bool Hide = false;
        public int TimeOut = -1;
    }
}
