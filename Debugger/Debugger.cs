using System;

namespace FOG {
	class Program {
		public static void Main(string[] args) {
			var snapin = new SnapinClient();
			
			LogHandler.setConsoleMode(true);
			CommunicationHandler.GetAndSetServerAddress();
			
			LogHandler.NewLine();
			const string authentication = "Authentication-Snapin";
			LogHandler.PaddedHeader(authentication);
			LogHandler.NewLine();
			CommunicationHandler.Authenticate();
			snapin.start();
			Console.ReadLine();
			
		}
	}
}