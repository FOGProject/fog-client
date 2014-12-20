using System;
using System.Collections.Generic;

namespace FOG {
	/// <summary>
	/// Contains the information that the FOG Server responds with
	/// </summary>
	public class Response {

		private Boolean error;
		private Dictionary<String, String> data;
		private String returnCode;
		
		public Response(Boolean error, Dictionary<String, String> data, String returnCode) {
			this.error = error;
			this.data = data;
			this.returnCode = returnCode;
		}

		public Response() {
			this.error = true;
			this.data = new Dictionary<String, String>();
			this.returnCode = "";
		}

		public void setError(Boolean error) { this.error = error; }
		public Boolean wasError() { return this.error; }

		public void setData(Dictionary<String, String> data) { this.data = data; }
		public Dictionary<String, String> getData() { return this.data; }
		public String getReturnCode() { return this.returnCode; }
		public void setReturnCode(String returnCode) { this.returnCode = returnCode; }

		public String getField(String id) {
			//Check if the field exists
			if(data.ContainsKey(id)) {
				return data[id];
			}

			//Return a blank string if the field does not exist
			return "";
		}
	}
}