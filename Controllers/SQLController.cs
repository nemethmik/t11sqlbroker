using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using t11sqlbroker.Models;

namespace t11sqlbroker.Controllers {
	public class SQLController : ApiController {
		// GET: api/SQL
		public HttpResponseMessage Get([FromBody]SQLQuery value) {
			try {
				var result = SAPB1.SQLQuery(value);
				return Request.CreateResponse<SQLResult>(result.statusCode, result);
			} catch (Exception e) {
				var result = new SQLResult {statusCode = HttpStatusCode.BadRequest, errorCode = e.HResult, errorText = e.Message, errorStackTrace = e.StackTrace };
				return Request.CreateResponse<SQLResult>(result.statusCode, result);
			}
		}

		//GET: api/SQL/DISCONNECT
		[Route("api/SQL/{command}")]
		public string Get([FromUri]string command) {
			if (command.Equals("DISCONNECT")) {
				try {
					DIConnection.Me.Dispose();
					return command + " Done";
				} catch (Exception e) {
					return e.Message + " : " + e.StackTrace;
				}
			} return command + " Unknown";
		}

		// POST: api/SQL
		public HttpResponseMessage Post([FromBody]SQLQuery value) {
			try {
				var result = SAPB1.SQLQuery(value);
				return Request.CreateResponse<SQLResult>(result.statusCode, result);
			} catch (Exception e) {
				var result = new SQLResult {statusCode = HttpStatusCode.BadRequest, errorCode = -1, errorText = e.Message, errorStackTrace = e.StackTrace };
				return Request.CreateResponse<SQLResult>(result.statusCode, result);
			}
		}
	}
}
