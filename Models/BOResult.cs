using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace t11sqlbroker.Models {
	/// <summary>
	/// The result object for the HTTP response for BO Requests
	/// </summary>
	public class BOResult {
		/// <summary>
		/// The HTTP status code.
		/// </summary>
		public System.Net.HttpStatusCode statusCode;
		/// <summary>
		/// The error code, if there is an error.
		/// </summary>
		public int errorCode;
		/// <summary>
		/// If there is an error the error text.
		/// If not found the HTTP not Found status code is here
		/// </summary>
		public string errorText;
		/// <summary>
		/// Error stack trace for debugging purposes
		/// </summary>
		public string errorStackTrace;
		/// <summary>
		/// Returns if the object was found. For example after a deletion, no object is returned
		/// </summary>
		public bool found;
		/// <summary>
		/// The ID of the BO which was requested to be get/update/delete.
		/// ID is not returned after POST. It is available in the BO object returned ater addition.
		/// </summary>
		public string id;
		/// <summary>
		/// The milliseconds of the execution time for performance benchmarking purposes. 
		/// </summary>
		public int execMillis;
		/// <summary>
		/// Te BO data in JSON format
		/// </summary>
		public Newtonsoft.Json.Linq.JToken bo;
		/// <summary>
		/// The XML string of the BO, when requested
		/// </summary>
		public string rawXml;
		/// <summary>
		/// The XML schema of the BO, when requested
		/// </summary>
		public string xmlSchema;
		/// <summary>
		/// The effective connection details: Profile, CompanyDB, Server, DBUser, UserName.
		/// No passwords and no other details are returned.
		/// </summary>
		public ConnectionParams connection = new ConnectionParams();
	}
}