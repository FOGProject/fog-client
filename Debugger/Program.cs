using System;

namespace FOG {
	class Program {
		public static void Main(string[] args) {
			
			LogHandler.setConsoleMode(true);
			CommunicationHandler.GetAndSetServerAddress();
			
			LogHandler.NewLine();
			LogHandler.PaddedHeader("Authentication");
			
			Boolean auth = CommunicationHandler.Authenticate();
			
			LogHandler.Divider();
			LogHandler.NewLine();
			
			Console.Write("Press any key exit...");
			Console.ReadKey(true);		
			
		}
	}
}