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

namespace FOG.Handlers.Power
{
    interface IPower
    {
        /// <summary>
        ///     Shutdown the computer
        /// </summary>
        /// <param name="comment">The message to append to the request</param>
        /// <param name="options">The options the user has on the prompt form</param>
        /// <param name="message">The message to show in the shutdown gui</param>
        /// <param name="seconds">How long to wait before processing the request</param>
        void Shutdown(string comment, Power.FormOption options = Power.FormOption.Abort, string message = null,
            int seconds = 30);

        /// <summary>
        ///     Restart the computer
        /// </summary>
        /// <param name="comment">The message to append to the request</param>
        /// <param name="options">The options the user has on the prompt form</param>
        /// <param name="message">The message to show in the shutdown gui</param>
        /// <param name="seconds">How long to wait before processing the request</param>
        void Restart(string comment, Power.FormOption options = Power.FormOption.Abort, string message = null,
            int seconds = 30);

        /// <summary>
        ///     Entry off the current user
        /// </summary>
        void LogOffUser();

        /// <summary>
        ///     Hibernate the computer
        /// </summary>
        void Hibernate();

        /// <summary>
        ///     Lock the workstation
        /// </summary>
        void LockWorkStation();

        void CreateTask(string parameters);
    }
}
