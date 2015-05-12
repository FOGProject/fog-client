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
using System.IO;

namespace FOG.Handlers
{
    /// <summary>
    ///     Handle all interaction with the log file
    /// </summary>
    public static class LogHandler
    {
        public enum LogMode
        {
            File,
            Console
        }

        private const long DEFAULT_MAX_LOG_SIZE = 502400;
        private const int HEADER_LENGTH = 78;
        private const string LOG_NAME = "LogHandler";
        private static bool initialized = initialize();
        //Define variables
        public static string FilePath { get; set; }
        public static long MaxSize { get; set; }
        public static LogMode Mode { get; set; }

        private static bool initialize()
        {
            FilePath = @"\fog.log";
            MaxSize = DEFAULT_MAX_LOG_SIZE;
            Mode = LogMode.File;

            return true;
        }

        /// <summary>
        ///     Log a message
        /// </summary>
        /// <param name="caller">The name of the calling method or class</param>
        /// <param name="message">The message to log</param>
        public static void Log(string caller, string message)
        {
            WriteLine(string.Format(" {0} {1} {2} {3}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), caller, message));
        }

        /// <summary>
        ///     Write a new line to the log
        /// </summary>
        public static void NewLine()
        {
            WriteLine("");
        }

        /// <summary>
        ///     Write a divider to the log
        /// </summary>
        public static void Divider()
        {
            Header("");
        }

        /// <summary>
        ///     Write a header to the log
        /// </summary>
        /// <param name="text">The text to put in the center of the header</param>
        public static void Header(string text)
        {
            double headerSize = (HEADER_LENGTH - text.Length)/2;
            var output = "";
            for (var i = 0; i < (int) Math.Ceiling(headerSize); i++)
                output += "-";

            output += text;

            for (var i = 0; i < ((int) Math.Floor(headerSize)); i++)
                output += "-";
            WriteLine(output);
        }

        /// <summary>
        ///     Create one header with a divider above and below it
        /// </summary>
        /// <param name="text">The text to put in the center of the header</param>
        public static void PaddedHeader(string text)
        {
            Divider();
            Header(text);
            Divider();
        }

        /// <summary>
        ///     Write text to the log
        /// </summary>
        /// <param name="text">The text to write</param>
        public static void Write(string text)
        {
            if (Mode == LogMode.Console)
            {
                if (text.ToUpper().Contains("ERROR"))
                    Console.BackgroundColor = ConsoleColor.Red;
                Console.Write(text);
                Console.BackgroundColor = ConsoleColor.Black;
            }
            else
            {
                var logFile = new FileInfo(FilePath);

                //Delete the log file if it excedes the max log size
                if (logFile.Exists && logFile.Length > MaxSize)
                    cleanLog(logFile);

                try
                {
                    //Write message to log file
                    var logWriter = new StreamWriter(FilePath, true);
                    logWriter.Write(text);
                    logWriter.Close();
                }
                catch
                {
                    //If logging fails then nothing can really be done to silently notify the user
                }
            }
        }

        /// <summary>
        ///     Write a line to the log
        /// </summary>
        /// <param name="line">The line to write</param>
        public static void WriteLine(string line)
        {
            Write(line + "\r\n");
        }

        /// <summary>
        ///     Wipe the log
        /// </summary>
        /// <param name="logFile"></param>
        private static void cleanLog(FileSystemInfo logFile)
        {
            try
            {
                logFile.Delete();
            }
            catch (Exception ex)
            {
                Log(LOG_NAME, string.Format("Failed to delete log file: {0}", ex.Message));
            }
        }
    }
}