using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace t11sqlbroker.Models {
	/// <summary>
	/// The result of the HTTP response for an SQL Query request.
	/// </summary>
	public class SQLResult : IHttpActionResult {
		/// <summary>
		/// When column information was requested, some details about the columns
		/// </summary>
		public class Column {
			/// <summary>
			/// The column/field name
			/// </summary>
			public string name;
			/// <summary>
			/// The data type of the column: db_Alpha for strings, for example.
			/// For ADO.NET queries it's the database type name.
			/// </summary>
			public string dataType;
			/// <summary>
			/// The sub type of the field as defined by SAP.
			/// For ADO.NET queries, this is empty.
			/// </summary>
			public string subType;
			/// <summary>
			/// The valid value list. Most typical for Y/N columns
			/// For ADO.NET queries, this is empty.
			/// </summary>
			public List<ValidValue> validValues = new List<ValidValue>();
		}
		/// <summary>
		/// The actual valid values for a field where valid values are defined in SAP B1
		/// </summary>
		public class ValidValue {
			/// <summary>
			/// The display text of the fiield
			/// </summary>
			public string description;
			/// <summary>
			/// The code/key value of the list element
			/// </summary>
			public string value;
		}
		/// <summary>
		/// The HTTP status code.
		/// </summary>
		public System.Net.HttpStatusCode statusCode;
		/// <summary>
		/// The transaction was set to rollback-only, so all insert, update, delete attempts were rolled back eventually.
		/// </summary>
		public bool rollbackOnly;
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
		/// <summary>
		/// When requested the array of column information.
		/// </summary>
		public List<Column> columns = new List<Column>();
		//public Newtonsoft.Json.Linq.JArray rows = new Newtonsoft.Json.Linq.JArray();
		/// <summary>
		/// The details of the rows for ADO.NET requests
		/// </summary>
		//public Newtonsoft.Json.Linq.JToken rows;
		/// <summary>
		/// The query details for DI Recordset
		/// </summary>
		public Newtonsoft.Json.Linq.JToken data;
		/// <summary>
		/// The data in XML format for DI Recordset queries
		/// For ADO.NET queries no XML is returned.
		/// </summary>
		public string rawXml;
		/// <summary>
		/// The effective connection details: Profile, CompanyDB, Server, DBUser, UserName.
		/// No passwords and no other details are returned.
		/// </summary>
		public NoPwdConnectionParams connection = new NoPwdConnectionParams();
		/// <summary>
		/// The SQL string executed for user queries
		/// </summary>
		public string SQL;
		/// <summary>
		/// The actually executed user query prefixed with its category name
		/// JobWatch.X.GetOpenJobs, for example refers to that this was an extension function.
		/// </summary>
		public string userQuery;
		/// <summary>
		/// If an customised/extended version was found for the requested UQ, this is set to true
		/// </summary>
		public bool extendedUQ = false;
		private HttpRequestMessage Request;
		Task<HttpResponseMessage> IHttpActionResult.ExecuteAsync(CancellationToken cancellationToken) {
			if (Request == null) throw new SQLBrokerError("Request object not initialized",sqlResult:this);
			return Task.FromResult(Request.CreateResponse<SQLResult>(this.statusCode, this));
		}
		/// <summary>
		/// SQLResult must be initialized with HttpRequestMessage since it implements IHttpActionResult
		/// </summary>
		/// <param name="rm"></param>
		public SQLResult(HttpRequestMessage rm) {
			if (rm == null) throw new Exception("SQL Result must be initialized with HttpRequestMessage");
			Request = rm;
		}
		/// <summary>
		/// Call this function upon exception before returning HTTP response.
		/// </summary>
		/// <param name="e"></param>
		public SQLResult setResponseStatus(HttpStatusCode status,Exception e) {
			statusCode = status;
			if(errorCode == 0) errorCode = -1;
			if(string.IsNullOrEmpty(errorText)) errorText = e?.Message;
			errorStackTrace = e?.StackTrace;
			return this;
		}
	}
}