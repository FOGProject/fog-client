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
using System.Security.Cryptography;

namespace FOG.Handlers.Data
{
    public static class Generate
    {
        private const string LogName = "Data::Generate";

        /// <summary>
        ///     Securely generates a random number
        /// </summary>
        /// <param name="rngProvider">The RNG crypto provider used</param>
        /// <param name="min">The minimum value of the random number</param>
        /// <param name="max">The maximum value of the random number</param>
        /// <returns>A random number between min and max</returns>
        public static int Random(RNGCryptoServiceProvider rngProvider, int min, int max)
        {
            if (rngProvider == null) throw new ArgumentNullException(nameof(rngProvider));
            if (min > max) throw new ArgumentException("Min is greater than max");

            var b = new byte[sizeof (uint)];
            rngProvider.GetBytes(b);
            var d = BitConverter.ToUInt32(b, 0)/(double) uint.MaxValue;

            return min + (int) ((max - min)*d);
        }

        /// <summary>
        ///     Securely generates an alpha-numerical password
        /// </summary>
        /// <param name="length">The number of characters the password should contain</param>
        /// <returns>A randomly generated password</returns>
        public static string Password(int length)
        {
            if (length <= 0) return null;

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            var random = new RNGCryptoServiceProvider();

            for (var i = 0; i < stringChars.Length; i++)
                stringChars[i] = chars[Random(random, 1, chars.Length)];

            return new string(stringChars);
        }
    }
}