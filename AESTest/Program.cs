/*
 * Created by SharpDevelop.
 * User: Joseph
 * Date: 12/19/2014
 * Time: 1:43 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace FOG
{
	class Program
	{
		public static void Main(string[] args)
		{
			

			LogHandler.setConsoleMode(true);
			CommunicationHandler.getAndSetServerAddress();

			
			Console.WriteLine("Beginning auth test");
			Console.WriteLine();
			
			LogHandler.divider();
			for(int i = 0; i<= 0; i++) {
				CommunicationHandler.authenticate();
			}
			Console.WriteLine();
			LogHandler.divider();
			Console.Write("Press any key exit...");
			Console.ReadKey(true);			

		}
	}
}