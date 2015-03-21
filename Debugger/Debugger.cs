using System;

namespace FOG
{
    class Program
    {
        public static void Main(string[] args)
        {

            LogHandler.setConsoleMode(true);
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