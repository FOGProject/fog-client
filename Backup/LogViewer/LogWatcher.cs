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
using System.IO;

namespace FOG
{
    /// <summary>
    /// Watch log files
    /// </summary>
    public class LogWatcher : FileSystemWatcher
    {
        internal String FileName;
        FileStream stream;
        StreamReader reader;
        
        public LogWatcher(String fileName)
        {

            this.Changed += OnChanged;
            FileName = fileName;
            
            this.stream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            this.reader = new StreamReader(stream);
            
            stream.Position = stream.Length;
        }
        
        public void OnChanged(object obj, FileSystemEventArgs e) {
            String content = reader.ReadToEnd();
            var args = new LogWatcherEventArgs(content);
           
            if (TextChanged != null)
                TextChanged(this, args);

        }
        
        public delegate void LogWatcherEventHandler(object sender, LogWatcherEventArgs e);
        
        public event LogWatcherEventHandler TextChanged;
    }
    
    public class LogWatcherEventArgs : EventArgs
    {
        public String Content;
        
        public LogWatcherEventArgs(String content) 
        {
            this.Content = content;
        }
    }
        
}
