/*
 * Zazzles : A cross platform service framework
 * Copyright (C) 2014-2016 FOG Project
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

using Newtonsoft.Json;

namespace FOG.Modules.PrinterManager
{
    public class PrinterMessage
    {
        [JsonProperty(Required = Required.Always)]
        public string Name { get; private set; }

        [JsonProperty(Required = Required.Always)]
        public string File { get; private set; }

        [JsonProperty(Required = Required.Always)]
        public string Hash { get; private set; }

        [JsonProperty(Required = Required.Always)]
        public string RunWith { get; private set; }

        [JsonProperty(Required = Required.Always)]
        public string RunWithArgs { get; private set; }

        [JsonProperty(Required = Required.Always)]
        public string Args { get; private set; }

        [JsonProperty(Required = Required.Always)]
        public bool Restart { get; private set; }
    }
}
