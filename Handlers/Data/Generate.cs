using System;
using System.Security.Cryptography;

namespace FOG.Handlers.Data
{
    public static class Generate
    {
        private const string LogName = "Data::Generate";

        /// <summary>
        ///     Securley generates a random number
        ///     <param name="rngProvider">The RNG crypto provider used</param>
        ///     <param name="min">The minimum value of the random number</param>
        ///     <param name="max">The maximum value of the random number</param>
        ///     <returns>A random number between min and max</returns>
        /// </summary>
        public static int Random(RNGCryptoServiceProvider rngProvider, int min, int max)
        {
            if (rngProvider == null) throw new ArgumentNullException("rngProvider");
            if (min > max) throw new ArgumentException("Min is greater than max");

            var b = new byte[sizeof(uint)];
            rngProvider.GetBytes(b);
            var d = BitConverter.ToUInt32(b, 0) / (double)uint.MaxValue;

            return min + (int)((max - min) * d);
        }

        /// <summary>
        ///     Securely generates an alpha-numerical password
        ///     <param name="length">The number of characters the password should contain</param>
        ///     <returns>A randomly generated password</returns>
        /// </summary>
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
