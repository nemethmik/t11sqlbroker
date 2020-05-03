using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using t11sqlbroker.Models;

namespace t11sqlbroker.Controllers {
	public class SQLController : ApiController {
		void checkSQLReqParameter(SQLQuery value) {
			if (value == null) throw new Exception("The body is missing or not formatted correctly. Maybe just a comma is missing between two attributes.");
		}
		// GET: api/SQL
		[Route("api/SQL")]
		public HttpResponseMessage Get([FromBody]SQLQuery value) {
			try {
				checkSQLReqParameter(value);
				var result = SAPB1.SQLQuery(value,true);
				return Request.CreateResponse<SQLResult>(result.statusCode, result);
			} catch (Exception e) {
				var result = new SQLResult {statusCode = HttpStatusCode.BadRequest, errorCode = e.HResult, errorText = e.Message, errorStackTrace = e.StackTrace };
				return Request.CreateResponse<SQLResult>(result.statusCode, result);
			}
		}

		//GET: api/SQL/DISCONNECT
		[Route("api/SQL/{command}/{profile}")]
		public HttpResponseMessage Get([FromUri]string command,string profile) {
			if (command.Equals("Disconnect")) {
				try {
					DIConnection.Me.Dispose();
					return Request.CreateResponse(command + " Done");
				} catch (Exception e) {
					return Request.CreateErrorResponse(HttpStatusCode.BadRequest, e.Message + " : " + e.StackTrace);
				}
			} else if (command.Equals("GetConfig")) {
				try {
					var cp = ConnectionParams.GetConnectionProfile(profile);//"SQLBrokerDefault"
					//Passwords are removed for security resons.
					cp.AdoNetPassword = "********";
					cp.Password = "********";
					cp.DbPassword = "********";
					return Request.CreateResponse<ConnectionParams>(HttpStatusCode.OK,cp);
				} catch (Exception e) {
					return Request.CreateErrorResponse(HttpStatusCode.BadRequest, e.Message + " : " + e.StackTrace);
				}
			} else {
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, command + " Unknown");
			}
		}

		// POST: api/SQL
		[Route("api/SQL")]
		public HttpResponseMessage Post([FromBody]SQLQuery value) {
			try {
				checkSQLReqParameter(value);
				var result = SAPB1.SQLQuery(value,false);
				return Request.CreateResponse<SQLResult>(result.statusCode, result);
			} catch (Exception e) {
				var result = new SQLResult {statusCode = HttpStatusCode.BadRequest, errorCode = -1, errorText = e.Message, errorStackTrace = e.StackTrace };
				return Request.CreateResponse<SQLResult>(result.statusCode, result);
			}
		}
	}
}
