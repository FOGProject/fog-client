using System;

namespace FOG
{
    class Program
    {
        public static void Main(string[] args)
        {

            LogHandler.Mode = LogHandler.LogMode.Console;
            CommunicationHandler.GetAndSetServerAddress();

            LogHandler.NewLine();
            const string authentication = "Authentication-Snapin";

            LogHandler.PaddedHeader(authentication);
            LogHandler.NewLine();
            CommunicationHandler.Authenticate();
            LogHandler.NewLine();

            Console.ReadLine();

        }
    }
}