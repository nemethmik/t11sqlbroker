using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace t11sqlbroker.Models {
	/// <summary>
	/// The BO request data for GET, POST, PUT and DELETE requests.
	/// The HTTP response codes:
	/// GET - 404 Not Found, 200 OK
	/// POST - 201 Created (or 200 OK)
	/// DELETE - 410 Gone (or 200 OK), 404 Not Found
	/// PUT - 200 OK, 404 Not Found, Not Modified is never returned because of the limitations of DI.
	/// </summary>
	public class BORequest {
		/// <summary>
		/// Optional connection parameters, when the server is not preconfigured, 
		/// the client can send in the connection parameters.
		/// </summary>
		public ConnectionParams connection;
		/// <summary>
		/// If the BO name is not defined on the URI then this an alternative possibility
		/// </summary>
		//public string boName;
		/// <summary>
		/// GET (default) | ADD or POST | UPDATE or PUT | DELETE
		/// </summary>
		//public string command;
		/// <summary>
		/// The timeout of the query in seconds
		/// </summary>
		public int timeOut;
		/// <summary>
		/// When true the XML text from SAP is returned as is.
		/// </summary>
		public bool rawXml;
		/// <summary>
		/// The BO XML to create or update the DI object.
		/// Don't send boXml for DELETE requests, otherwise your request is rejected.
		/// Normally, just send JSON BO it is converted by the server automatically for SAP DI API XML.
		/// This is in case of emergency when something is not OK with the JSON - XML conversion.
		/// XML - JSON conversion is done by Newtosoft's JSON tools, which is anyway included in ASP.NET projects.
		/// </summary>
		public string boXml;
		/// <summary>
		/// The BO data in JSON format.
		/// Don't send BO JSON data for DELETE requests, otherwise your request is rejected.
		/// </summary>
		public Newtonsoft.Json.Linq.JObject bo;
		/// <summary>
		/// The caller wants the XML Schema for the BO, too.
		/// </summary>
		public bool xmlSchema;
	}
}