
using System;
using System.Net;

namespace FOG {
	/// <summary>
	/// Register the host with FOG
	/// </summary>
	public class HostRegister: AbstractModule {
		public HostRegister():base(){
			setName("HostRegister");
			setDescription("Register the host with the FOG server");
		}
	
		protected override void doWork() {
			LogHandler.Log(getName(), "Sending host information to FOG");
			CommunicationHandler.Contact("/service/register.php?mac=" + CommunicationHandler.GetMacAddresses() + "&hostname=" + Dns.GetHostName());
			
		}
		
	}
	
}