
using System;

namespace FOG {
	class Program {
		//A CLI based pipe-client used for sending out messages to the user-service from external programs
		private static PipeClient servicePipe;
		
		public static void Main(string[] args) {
			servicePipe = new PipeClient("fog_pipe_service");
			servicePipe.connect();		

			String allArgs = "";
			foreach(String arg in args) {
				allArgs = allArgs + arg + " ";				
			}
			
			servicePipe.sendMessage("SNG:" + allArgs);

		}
	}
}