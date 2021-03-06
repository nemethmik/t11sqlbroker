﻿using System;
using System.Collections.Generic;
using System.Data.Sql;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace t11sqlbroker.Models {
	public class MultiResult : IHttpActionResult {
		public int totalJobsRequested;
		public int index;
		/// <summary>
		/// The HTTP status code.
		/// </summary>
		public System.Net.HttpStatusCode statusCode = HttpStatusCode.OK;
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
		/// The milliseconds of the execution time for performance benchmarking purposes. 
		/// </summary>
		public int execMillis;
		public NoPwdConnectionParams connection = new NoPwdConnectionParams();
		public MReqResult errorResult;
		//public MReqJob reqJob;
		/// <summary>
		/// The array of the results of the requested jobs
		/// </summary>
		public List<MReqResult> results = new List<MReqResult>();
		private HttpRequestMessage Request;
		public MultiResult(HttpRequestMessage rm) {
			Request = rm;
		}
		Task<HttpResponseMessage> IHttpActionResult.ExecuteAsync(CancellationToken cancellationToken) {
			if (Request == null) throw new SQLBrokerError("Request object not initialized");
			return Task.FromResult(Request.CreateResponse<MultiResult>(this.statusCode, this));
		}
		public MultiResult setResponseStatus(HttpStatusCode status, Exception e) {
			statusCode = status;
			if (errorCode == 0) errorCode = -1;
			if (string.IsNullOrEmpty(errorText)) errorText = e?.Message;
			errorStackTrace = e?.StackTrace;
			return this;
		}
	}
	public class MReqResult {
		/// <summary>
		/// GET, PUT/Update, POST/Add, DELETE/Delete/Cancel 
		/// </summary>
		public SQLResult sqlResult;
		public BOResult boResult;
	}
	/// <summary>
	/// The result object for the HTTP response for BO Requests
	/// </summary>
	public class BOResult : IHttpActionResult {
		/// <summary>
		/// In multi-jobs requests the job index number starting with 0
		/// </summary>
		public int jobNumber;
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
		public NoPwdConnectionParams connection = new NoPwdConnectionParams();
		/// <summary>
		/// GET, PUT/Update, POST/Add, DELETE/Delete/Cancel 
		/// </summary>
		public string reqType;
		/// <summary>
		/// If the BO name is not defined on the URI then this an alternative possibility
		/// </summary>
		public string boName;
		private HttpRequestMessage Request;
		public BOResult(HttpRequestMessage rm) {
			Request = rm;
		}
		Task<HttpResponseMessage> IHttpActionResult.ExecuteAsync(CancellationToken cancellationToken) {
			if (Request == null) throw new SQLBrokerError("Request object not initialized", boResult: this);
			return Task.FromResult(Request.CreateResponse<BOResult>(this.statusCode, this));
		}
		public BOResult setResponseStatus(HttpStatusCode status, Exception e) {
			statusCode = status;
			if (errorCode == 0) errorCode = -1;
			if (string.IsNullOrEmpty(errorText)) errorText = e?.Message;
			errorStackTrace = e?.StackTrace;
			return this;
		}
	}
}