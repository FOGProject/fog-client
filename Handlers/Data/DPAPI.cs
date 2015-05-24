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

using System.Security.Cryptography;

namespace FOG.Handlers.Data
{
    public static class DPAPI
    {
        private const string LogName = "EncryptionHandler";

        /// <summary>
        /// Securly protect bytes by using local windows credentials
        /// </summary>
        /// <param name="data">The bytes to protect</param>
        /// <param name="userScope">Encrypt the data as the current user (false means use the local machine)</param>
        /// <returns></returns>
        public static byte[] ProtectData(byte[] data, bool userScope)
        {
            return ProtectedData.Protect(data, null, userScope ? DataProtectionScope.CurrentUser : DataProtectionScope.LocalMachine);
        }

        /// <summary>
        /// Unprotect bytes by using local windows credentials
        /// </summary>
        /// <param name="data">The bytes to unprotect</param>
        /// <param name="userScope">Decrypt the data as the current user (false means use the local machine)</param>
        /// <returns></returns>
        public static byte[] UnProtectData(byte[] data, bool userScope)
        {
            return ProtectedData.Unprotect(data, null, userScope ? DataProtectionScope.CurrentUser : DataProtectionScope.LocalMachine);
        }
    }
}
