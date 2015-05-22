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
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FOG.Handlers.Encryption
{
    public static class RSA
    {
        private const string LogName = "EncryptionHandler";

        /// <summary>
        /// Encrypt data using RSA
        /// </summary>
        /// <param name="cert">The X509 certificate to use</param>
        /// <param name="data">The data to encrypt</param>
        /// <returns>A hex string of the encrypted data</returns>
        public static string Encrypt(X509Certificate2 cert, string data)
        {
            var byteData = Encoding.UTF8.GetBytes(data);
            var encrypted = Encrypt(cert, byteData);
            return Transform.ByteArrayToHexString(encrypted);
        }

        /// <summary>
        /// Decrypt data using RSA
        /// </summary>
        /// <param name="cert">The X509 certificate to use</param>
        /// <param name="data">The data to decrypt</param>
        /// <returns>A UTF8 string of the data</returns>
        public static string Decrypt(X509Certificate2 cert, string data)
        {
            var byteData = Transform.HexStringToByteArray(data);
            var decrypted = Decrypt(cert, byteData);
            return Encoding.UTF8.GetString(decrypted);
        }

        /// <summary>
        /// Encrypt data using RSA
        /// </summary>
        /// <param name="cert">The X509 certificate to use</param>
        /// <param name="data">The data to encrypt</param>
        /// <returns>A byte array of the encrypted data</returns>
        public static byte[] Encrypt(X509Certificate2 cert, byte[] data)
        {
            if (cert == null)
                return null;

            var rsa = (RSACryptoServiceProvider)cert.PublicKey.Key;
            return rsa.Encrypt(data, true);
        }

        /// <summary>
        /// Decrypt data using RSA
        /// </summary>
        /// <param name="cert">The X509 certificate to use</param>
        /// <param name="data">The data to decrypt</param>
        /// <returns>A byte array of the decrypted data</returns>
        public static byte[] Decrypt(X509Certificate2 cert, byte[] data)
        {
            if (cert == null || !cert.HasPrivateKey)
                return null;

            var rsa = (RSACryptoServiceProvider)cert.PrivateKey;
            return rsa.Decrypt(data, true);
        }

        /// <summary>
        /// Validate that certificate came from a specific CA
        /// http://stackoverflow.com/a/17225510/4732290
        /// </summary>
        /// <param name="authority">The CA certificate</param>
        /// <param name="certificate">The certificate to validate</param>
        /// <returns>True if the certificate came from the authority</returns>
        public static bool IsFromCA(X509Certificate2 authority, X509Certificate2 certificate)
        {
            var chain = new X509Chain
            {
                ChainPolicy =
                {
                    RevocationMode = X509RevocationMode.NoCheck,
                    RevocationFlag = X509RevocationFlag.ExcludeRoot,
                    VerificationTime = DateTime.Now,
                    UrlRetrievalTimeout = new TimeSpan(0, 0, 0)
                }
            };

            // This part is very important. You're adding your known root here.
            // It doesn't have to be in the computer store at all. Neither certificates do.
            chain.ChainPolicy.ExtraStore.Add(authority);

            var isChainValid = chain.Build(certificate);

            if (!isChainValid)
            {
                var errors = chain.ChainStatus
                    .Select(x => string.Format("{0} ({1})", x.StatusInformation.Trim(), x.Status))
                    .ToArray();
                var certificateErrorsString = "Unknown errors.";

                if (errors != null && errors.Length > 0)
                    certificateErrorsString = string.Join(", ", errors);

                LogHandler.Error(LogName, "Certificate validation failed");
                LogHandler.Error(LogName, "Trust chain did not complete to the known authority anchor. Errors: " + certificateErrorsString);
                return false;
            }

            // This piece makes sure it actually matches your known root
            if (chain.ChainElements.Cast<X509ChainElement>().Any(x => x.Certificate.Thumbprint == authority.Thumbprint))
                return true;

            LogHandler.Error(LogName, "Certificate validation failed");
            LogHandler.Error(LogName, "Trust chain did not complete to the known authority anchor. Thumbprints did not match.");
            return false;
        }

        /// <summary>
        /// </summary>
        /// <returns>The FOG CA root certificate</returns>
        public static X509Certificate2 GetCACertificate()
        {
            try
            {
                X509Certificate2 CAroot = null;
                var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);
                var cers = store.Certificates.Find(X509FindType.FindBySubjectName, "FOG Server CA", true);

                if (cers.Count > 0)
                {
                    LogHandler.Log(LogName, "CA cert found");
                    CAroot = cers[0];
                }
                store.Close();

                return CAroot;
            }
            catch (Exception ex)
            {
                LogHandler.Error(LogName, "Unable to get CA");
                LogHandler.Error(LogName, ex);
            }

            return null;
        }
    }
}
