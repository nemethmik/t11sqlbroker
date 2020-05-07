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
			if (string.IsNullOrEmpty(value.SQL) && string.IsNullOrEmpty(value.userQuery)) throw new Exception("No SQL nor userQuery defined. Maybe your JSON is badly formatted. Use JSON.stringify");
		}
		SQLResult handleException(Exception e) {
			var r = (e is SQLBrokerError && (e as SQLBrokerError).sqlResult != null) ? (e as SQLBrokerError).sqlResult : new SQLResult(Request);
			return r.setResponseStatus(HttpStatusCode.BadRequest, e);
		}
		[Route("api/UQ/{category}/{name}/{profile?}/{timeOut?}/{columnInfo?}/{p0?}/{p1?}/{p2?}/{p3?}/{p4?}/{p5?}/{p6?}/{p7?}/{p8?}/{p9?}")]
		public SQLResult Get(string category,string name, string profile = null, int timeOut=0, bool columnInfo = false
			, string p0 = null, string p1 = null, string p2 = null, string p3 = null, string p4 = null
			, string p5 = null, string p6 = null, string p7 = null, string p8 = null, string p9 = null) {
			try {
				if (!SAPB1.brokerConf.uq) throw new Exception("User Query module was disabled in web.config for SQL Broker");
				if (string.IsNullOrEmpty(name)) throw new Exception("No name was defined for User Query");
				SQLQuery q = new SQLQuery { userQuery = name, columnInfo = columnInfo, uqCategory = category };
				if (!string.IsNullOrEmpty(profile)) q.connection = new ConnectionParams { Profile = profile };
				q.timeOut = timeOut;
				q.parameters = new string[10];
				q.parameters[0] = p0; q.parameters[1] = p1; q.parameters[2] = p2; q.parameters[3] = p3; q.parameters[4] = p4;
				q.parameters[5] = p5; q.parameters[6] = p6; q.parameters[7] = p7; q.parameters[8] = p8; q.parameters[9] = p9;
				checkSQLReqParameter(q);
				return SAPB1.SQLQuery(q, true,new SQLResult(Request));
			} catch (Exception e) {
				return handleException(e);
			}
		}

		// GET: api/SQL
		[Route("api/SQL")]
		public SQLResult Get([FromBody]SQLQuery value) {
			try {
				if (!SAPB1.brokerConf.sql) throw new Exception("SQL module was disabled in web.config for SQL Broker");
				checkSQLReqParameter(value);
				return SAPB1.SQLQuery(value,true,new SQLResult(Request));
			} catch (Exception e) {
				return handleException(e);
			}
		}

		//GET: api/SQL/Disconnect
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
					return Request.CreateResponse<NoPwdConnectionParams>(HttpStatusCode.OK, new NoPwdConnectionParams(cp));
				} catch (Exception e) {
					return Request.CreateErrorResponse(HttpStatusCode.BadRequest, e.Message + " : " + e.StackTrace);
				}
			} else {
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, command + " Unknown");
			}
		}

		// POST: api/SQL
		[Route("api/SQL")]
		public SQLResult Post([FromBody]SQLQuery value) {
			try {
				if (!SAPB1.brokerConf.sql) throw new Exception("SQL module was disabled in web.config for SQL Broker");
				checkSQLReqParameter(value);
				return SAPB1.SQLQuery(value, false, new SQLResult(Request));
			} catch (Exception e) {
				return handleException(e);
			}
		}
	}
}
