using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace t11sqlbroker.Models {
	/// <summary>
	/// The Query command object
	/// </summary>
	public class SQLQuery {
		/// <summary>
		/// Optional connection parameters, when the server is not preconfigured, 
		/// the client can send in the connection parameters.
		/// </summary>
		public ConnectionParams connection;
		/// <summary>
		/// The SQL SELECT statement
		/// </summary>
		public string SQL;
		/// <summary>
		/// The number of rows returned by the query, you'd better use SELECT TOP, though.
		/// </summary>
		public int maxRows;
		/// <summary>
		/// The timeout of the query in seconds
		/// </summary>
		public int timeOut;
		/// <summary>
		/// When true the XML text from SAP is returned as is.
		/// </summary>
		public bool rawXml;
		/// <summary>
		/// When, true column info is added along with the rows.
		/// This adds 3 - 5 seconds extra to the total execution time.
		/// </summary>
		public bool columnInfo;
		/// <summary>
		/// Parameter array for user queries
		/// </summary>
		public string[] parameters;
		/// <summary>
		/// Name of the user quer
		/// </summary>
		public string userQuery;
		/// <summary>
		/// The category name of the user query
		/// </summary>
		public string uqCategory;
		/// <summary>
		/// The language of customized messages
		/// </summary>
		public string lang;
	}
}