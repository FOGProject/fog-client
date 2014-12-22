using System;

namespace FOG {
	class Program {
		public static void Main(string[] args) {
			
			LogHandler.setConsoleMode(true);
			CommunicationHandler.getAndSetServerAddress();
			
			LogHandler.newLine();
			LogHandler.paddedHeader("Authentication");
			
			Boolean auth = CommunicationHandler.authenticate();
			
			LogHandler.divider();
			LogHandler.newLine();
			
			Console.Write("Press any key exit...");
			Console.ReadKey(true);		
			
		}
	}
}