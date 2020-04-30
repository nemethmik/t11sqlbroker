using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using t11sqlbroker.Models;

namespace t11sqlbroker.Controllers {
	public class BOController : ApiController {
		// GET: api/BO
		//public IEnumerable<string> Get()
		//{
		//    return new string[] { "value1", "value2" };
		//}

		// GET: api/BO/name
		[Route("api/BO/{name}/{id}")]
		public BOResult Get([FromBody]BORequest boReq, string name, string id) {
			//return $"{name}({id}) GET";
			try {
				return SAPB1.BORequest(boReq,name,id);
			} catch (Exception e) {
				return new BOResult { errorCode = -1, errorText = e.Message };
			}
		}

		// POST: api/BO/ProductionOrder - to create a BO
		[Route("api/BO/{name}")]
		public string Post([FromBody]string value, string name) {
			return $"{name} with ({value}) CREATED";
		}

		// PUT: api/BO/ProductionOrder/6756 - to update
		[Route("api/BO/{name}/{id}")]
		public string Put([FromBody]string value, string name, string id) {
			return $"{name}({id}) with {value} UPDATED";
		}

		// DELETE: api/BO/5
		[Route("api/BO/{name}/{id}")]
		public string Delete([FromBody]string value, string name, string id) {
			return $"{name}({id}) DELETED";
		}
	}
}
