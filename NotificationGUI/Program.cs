
using System;
using System.Windows.Forms;
using FOG.Handlers;

namespace FOG {
	/// <summary>
	/// Class with program entry point.
	/// </summary>
	internal sealed class Program {
		/// <summary>
		/// Program entry point.
		/// </summary>
		[STAThread]
		private static void Main(string[] args) {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm(args));

		    Log.Output = Log.Mode.Quiet;
            Eager.Initalize();
		}
		
	}
}
