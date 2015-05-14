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

        public enum Level
        {
            Normal,
            Debug,
            Error
        }

        public enum LogMode
        {
            File,
            Console,
            Testing
        }

        private const long DefaultMaxLogSize = 502400;
        private const int HeaderLength = 78;
        private const string LogName = "LogHandler";
        private static bool _initialized = Initialize();

        public static string FilePath { get; set; }
        public static long MaxSize { get; set; }
        public static LogMode Mode { get; set; }
        public static bool Verbose { get; set; }

        private static bool Initialize()
        {
            FilePath = @"\fog.log";
            MaxSize = DefaultMaxLogSize;
            Mode = LogMode.File;

            return true;
        }

        /// <summary>
        ///     Log a message
        /// </summary>
        /// <param name="level">The logging level</param>
        /// <param name="caller">The name of the calling method or class</param>
        /// <param name="message">The message to log</param>
        public static void Log(Level level, string caller, string message)
        {
            #if DEBUG
            #else
                if (!Verbose && level == Level.Debug) return;
            #endif

            var prefix = "";

            if (level == Level.Debug || level == Level.Error)
                prefix = level.ToString().ToUpper()+": ";

            WriteLine(level, string.Format(" {0} {1} {2} {3}{4}",
                DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), caller, prefix, message));
        }

        /// <summary>
        ///     Log a message
        /// </summary>
        /// <param name="caller">The name of the calling method or class</param>
        /// <param name="message">The message to log</param>
        public static void Log(string caller, string message)
        {
            Log(Level.Normal, caller, message);
        }

        public static void Error(string caller, string message)
        {
            Log(Level.Error, caller, message);
        }

        public static void Error(string caller, Exception ex)
        {
            if (Mode == LogMode.Testing)
                throw ex;

            Log(Level.Error, caller, ex.Message);
        }

        public static void Debug(string caller, string message)
        {
            Log(Level.Debug, caller, message);
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
            var headerSize = (double)((HeaderLength - text.Length))/2;
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
        /// <param name="level">The logging level</param>
        /// <param name="text">The text to write</param>
        public static void Write(Level level, string text)
        {
            if (Mode == LogMode.Console)
            {
                if (level == Level.Error)
                    Console.BackgroundColor = ConsoleColor.Red;
                if (level == Level.Debug)
                    Console.BackgroundColor = ConsoleColor.Blue;

                Console.Write(text);
                Console.BackgroundColor = ConsoleColor.Black;
            }
            else
            {
                var logFile = new FileInfo(FilePath);

                //Delete the log file if it excedes the max log size
                if (logFile.Exists && logFile.Length > MaxSize)
                    CleanLog(logFile);

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
        ///     Write text to the log
        /// </summary>
        /// <param name="text">The text to write</param>
        public static void Write(string text)
        {
            Write(Level.Normal, text);
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
        ///     Write a line to the log
        /// </summary>
        /// <param name="line">The line to write</param>
        /// <param name="level">The logging level</param>
        public static void WriteLine(Level level, string line)
        {
            Write(level, line + "\r\n");
        }


        public static void UnhandledException(object sender, UnhandledExceptionEventArgs ex)
        {
            Log(LogName, "Unhandled exception caught");
            Log(LogName, string.Format("    Terminating: {0}", ex.IsTerminating));
            Log(LogName, string.Format("    Hash code: {0}", ex.ExceptionObject.GetHashCode()));
        }

        /// <summary>
        ///     Wipe the log
        /// </summary>
        /// <param name="logFile"></param>
        private static void CleanLog(FileSystemInfo logFile)
        {
            try
            {
                logFile.Delete();
            }
            catch (Exception ex)
            {
                Error(LogName, "Failed to delete log file");
                Error(LogName, ex.Message);
            }
        }
    }
}