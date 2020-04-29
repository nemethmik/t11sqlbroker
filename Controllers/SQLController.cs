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
		public SQLResult Get([FromBody]SQLQuery value) {
			try {
				return SAPB1.SQLQuery(value);
			} catch (Exception e) {
				return new SQLResult { errorCode = -1, errorText = e.Message };
			}
		}

		//GET: api/SQL/DISCONNECT
		public string Get(string command) {
			if (command.Equals("DISCONNECT")) {
				DIConnection.Me.Dispose();
				return command + " Done";
			} return command + " Unknown";
		}

		// POST: api/SQL
		public SQLResult Post([FromBody]SQLQuery value) {
			try {
				return SAPB1.SQLQuery(value);
			} catch (Exception e) {
				return new SQLResult { errorCode = -1, errorText = e.Message };
			}
		}

		// PUT: api/SQL/5
		//public void Put(int id, [FromBody]string value) {
		//}

		// DELETE: api/SQL/5
		//public void Delete(int id) {
		//}
	}
}
