using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using t11sqlbroker.Models;

namespace t11sqlbroker.Controllers {
	public class BOController : ApiController {
		// GET: api/BO/name
		[Route("api/BO/{name}/{id}")]
		public BOResult Get([FromBody]BORequest boReq, string name, string id) {
			try {
				return SAPB1.BORequest(q:boReq,name:name,id:id);
			} catch (Exception e) {
				return new BOResult { errorCode = -1, errorText = e.Message, errorStackTrace = e.StackTrace };
			}
		}

		// POST: api/BO/ProductionOrder - to create a BO
		[Route("api/BO/{name}")]
		public BOResult Post([FromBody]BORequest boReq, string name) {
			try {
				BOResult result = SAPB1.BORequest(q:boReq, name:name, id:null, post:true);
				return result;
			} catch (Exception e) {
				return new BOResult { errorCode = -1, errorText = e.Message, errorStackTrace = e.StackTrace };
			}
		}

		// PUT: api/BO/ProductionOrder/6756 - to update
		[Route("api/BO/{name}/{id}")]
		public BOResult Put([FromBody]BORequest boReq, string name, string id) {
			try {
				return SAPB1.BORequest(q: boReq, name: name, id: id, put: true);
			} catch (Exception e) {
				return new BOResult { errorCode = -1, errorText = e.Message, errorStackTrace = e.StackTrace };
			}
		}

		// DELETE: api/BO/5
		[Route("api/BO/{name}/{id}")]
		public BOResult Delete([FromBody]BORequest boReq, string name, string id) {
			try {
				return SAPB1.BORequest(q: boReq, name: name, id: id, delete: true);
			} catch (Exception e) {
				return new BOResult { errorCode = -1, errorText = e.Message, errorStackTrace = e.StackTrace };
			}
		}
	}
}
