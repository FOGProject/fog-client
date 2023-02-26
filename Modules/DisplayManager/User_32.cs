﻿/*
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

using System.Runtime.InteropServices;

namespace FOG.Modules.DisplayManager
{
    public static class User32
    {
        public const int EnumCurrentSettings = -1;
        public const int CdsUpdateregistry = 0x01;
        public const int CdsTest = 0x02;
        public const int DispChangeSuccessful = 0;
        public const int DispChangeRestart = 1;
        public const int DispChangeFailed = -1;

        /// <summary>
        ///     Populate a given display's configuration
        /// </summary>
        /// <param name="deviceName">The name of the display</param>
        /// <param name="modeNum"></param>
        /// <param name="settings">The variable to populate the configuration to</param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int EnumDisplaySettings(string deviceName, int modeNum, ref Devmode1 settings);

        /// <summary>
        ///     Adjust a given display's configuration
        /// </summary>
        /// <param name="settings">The settings to resize to</param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int ChangeDisplaySettings(ref Devmode1 settings, int flags);

        [StructLayout(LayoutKind.Sequential)]
        public struct Devmode1
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;

            public short dmOrientation;
            public short dmPaperSize;
            public short dmPaperLength;
            public short dmPaperWidth;

            public short dmScale;
            public short dmCopies;
            public short dmDefaultSource;
            public short dmPrintQuality;
            public short dmColor;
            public short dmDuplex;
            public short dmYWindowsFormsApplication1;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmFormName;
            public short dmLogPixels;
            public short dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;

            public int dmDisplayFlags;
            public int dmDisplayFrequency;

            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;

            public int dmPanningWidth;
            public int dmPanningHeight;
        };
    }
}