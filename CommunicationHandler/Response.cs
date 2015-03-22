using System;
using System.Collections.Generic;

namespace FOG {
	/// <summary>
	/// Contains the information that the FOG Server responds with
	/// </summary>
	public class Response {

        public Boolean Error { get; set; }
        public Dictionary<String, String> Data { get; set; }
        public String ReturnCode { get; set; }
		
		public Response(Boolean error, Dictionary<String, String> data, String returnCode) {
			Error = error;
			Data = data;
			ReturnCode = returnCode;
		}

		public Response() {
			Error = true;
			Data = new Dictionary<String, String>();
			ReturnCode = "";
		}
        
        /// <summary>
        /// Return the value stored at a specified key
        /// </summary>
        /// <param name="id">The ID to return</param>
        /// <returns>The value stored at key ID, if the ID is not present, return an empty string</returns>
		public String getField(String id) {
            return Data.ContainsKey(id) ? Data[id] : "";
		}
	}
}