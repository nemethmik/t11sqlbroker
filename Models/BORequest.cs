using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace t11sqlbroker.Models {
	/// <summary>
	/// The Query command object
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
		public string boName;
		/// <summary>
		/// GET (default) | ADD or POST | UPDATE or PUT | DELETE
		/// </summary>
		public string command;
		/// <summary>
		/// The timeout of the query in seconds
		/// </summary>
		public int timeOut;
		/// <summary>
		/// When true the XML text from SAP is returned as is.
		/// </summary>
		public bool rawXml;
	}
}