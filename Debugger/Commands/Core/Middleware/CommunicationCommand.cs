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

using FOG.Handlers;
using FOG.Handlers.Middleware;

namespace FOG.Commands.Core.Middleware
{
    class CommunicationCommand : ICommand
    {
        private const string LogName = "Console::Middleware::Communication";

        public bool Process(string[] args)
        {
            if (args.Length < 2) return false;
            
            if (args[0].Equals("contact"))
            {
                var success = Communication.Contact(args[1]);
                Log.Entry(LogName, "Passed: " + success);
                return true;
            }

            if (args[0].Equals("response"))
            {
                var response = Communication.GetResponse(args[1]);
                response.PrettyPrint();
                return true;
            }

            if (args[0].Equals("raw-response"))
            {
                var response = Communication.GetRawResponse(args[1]);
                Log.Entry(LogName, "Response = " + response);

                return true;
            }

            if (args.Length <= 2) return false;

            if (args[0].Equals("post"))
            {
                var response = Communication.Post(args[1], args[2]);
                response.PrettyPrint();
                return true;
            }

            if (args[0].Equals("download"))
            {
                var success = Communication.DownloadFile(args[1], args[2]);
                Log.Entry(LogName, "Passed: " + success);
                return true;
            }

            if (args[0].Equals("download-external"))
            {
                var success = Communication.DownloadExternalFile(args[1], args[2]);
                Log.Entry(LogName, "Passed: " + success);
                return true;
            }

            return false;
        }
    }
}
