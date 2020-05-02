using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace t11sqlbroker.Models {
	public class ConfigResult {
		/// <summary>
		/// The HTTP status code.
		/// </summary>
		public System.Net.HttpStatusCode statusCode;
		/// <summary>
		/// The error code, if there is an error.
		/// </summary>
		public int errorCode;
		/// <summary>
		/// If there is an error the error text
		/// </summary>
		public string errorText;
		/// <summary>
		/// Error stack trace for debugging purposes
		/// </summary>
		public string errorStackTrace;
		/// <summary>
		/// The milliseconds of the execution time for performance benchmarking purposes. 
		/// </summary>
		public int execMillis;
		public string profile;
		public ConnectionParams connection;
	}
}