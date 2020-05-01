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
		public HttpResponseMessage Get([FromBody]BORequest boReq, string name, string id) {
			try {
				var result = SAPB1.BORequest(q:boReq,name:name,id:id);
				return Request.CreateResponse<BOResult>(result.statusCode, result);
			} catch (Exception e) {
				var result = new BOResult { statusCode = HttpStatusCode.BadRequest, errorCode = -1, errorText = e.Message, errorStackTrace = e.StackTrace };
				return Request.CreateResponse<BOResult>(result.statusCode, result);
			}
		}

		// POST: api/BO/ProductionOrder - to create a BO
		[Route("api/BO/{name}")]
		public HttpResponseMessage Post([FromBody]BORequest boReq, string name) {
			try {
				var result = SAPB1.BORequest(q:boReq, name:name, id:null, post:true);
				return Request.CreateResponse<BOResult>(result.statusCode, result);
			} catch (Exception e) {
				var result = new BOResult { statusCode = HttpStatusCode.BadRequest, errorCode = -1, errorText = e.Message, errorStackTrace = e.StackTrace };
				return Request.CreateResponse<BOResult>(result.statusCode, result);
			}
		}

		// PUT: api/BO/ProductionOrder/6756 - to update
		[Route("api/BO/{name}/{id}")]
		public HttpResponseMessage Put([FromBody]BORequest boReq, string name, string id) {
			try {
				var result = SAPB1.BORequest(q: boReq, name: name, id: id, put: true);
				return Request.CreateResponse<BOResult>(result.statusCode, result);
			} catch (Exception e) {
				var result = new BOResult { statusCode = HttpStatusCode.BadRequest, errorCode = -1, errorText = e.Message, errorStackTrace = e.StackTrace };
				return Request.CreateResponse<BOResult>(result.statusCode, result);
			}
		}

		// DELETE: api/BO/Activity/5
		[Route("api/BO/{name}/{id}")]
		public HttpResponseMessage Delete([FromBody]BORequest boReq, string name, string id) {
			try {
				BOResult result = SAPB1.BORequest(q: boReq, name: name, id: id, delete: true);
				return Request.CreateResponse<BOResult>(result.statusCode, result);
			} catch (Exception e) {
				var result = new BOResult { statusCode = HttpStatusCode.BadRequest, errorCode = -1, errorText = e.Message, errorStackTrace = e.StackTrace };
				return Request.CreateResponse<BOResult>(result.statusCode, result);					
			}
		}
	}
}
