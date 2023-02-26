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

namespace FOG.Modules.PowerManagement
{
    public class Task
    {
        public string CRON = "";
        public string Action = "";


        public override string ToString()
        {
            return $"{(Action ?? "")} at {(CRON ?? "")}";
        }

        public string ToQuartz()
        {
            const int dom = 2;
            const int dow = 4;

            // Prepend a 0 for the quartz seconds field
            var quartzFormat = "0 ";
            var unixCron = CRON.Split(' ');
            
            // If DOM is set, ignore DOW
            // else ignore DOM
            if (unixCron.Length < 5)
                quartzFormat += CRON;
            else
            {
                if (unixCron[dom] != "*")
                    unixCron[dow] = "?";
                else
                    unixCron[dom] = "?";

                // Correct for Day of Week being counted from
                // 1=Sunday by Quartz but 0=Sunday by CRON
                if (int.TryParse(unixCron[dow], out int dayofweek))
                    unixCron[dow] = (dayofweek + 1).ToString();
                quartzFormat += string.Join(" ", unixCron);
            }

            return quartzFormat;
        }
    }
}
