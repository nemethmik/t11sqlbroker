using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using t11sqlbroker.Models;

namespace t11sqlbroker.Controllers {
	public class BOController : ApiController {
		void checkBoReqParameter(BORequest boReq) {
			if (boReq == null) throw new Exception("The body is missing or not formatted correctly. Maybe just a comma is missing between two attributes.");
		}
		BOResult handleException(Exception e) {
			var r = (e is SQLBrokerError && (e as SQLBrokerError).boResult != null) ? (e as SQLBrokerError).boResult : new BOResult(Request);
			return r.setResponseStatus(HttpStatusCode.BadRequest, e);
		}
		void applyProfileFromURI(BORequest boReq, string profile, bool rawXml = false, bool xmlSchema = false) {
			if (!string.IsNullOrEmpty(profile)) {
				if (boReq.connection == null) boReq.connection = new ConnectionParams { Profile = profile };
				else if (string.IsNullOrEmpty(boReq.connection.Profile)) boReq.connection.Profile = profile;
			}
			if (rawXml) boReq.rawXml = rawXml;
			if (xmlSchema) boReq.xmlSchema = xmlSchema;
		}
		// GET: api/BO/name
		[Route("api/BO/{name}/{id}/{profile?}/{rawXml?}/{xmlSchema?}")]
		public BOResult Get(/*[FromBody]BORequest boReq,*/ string name, string id, string profile = null, bool rawXml = false, bool xmlSchema = false) {
			try {
				BORequest boReq = new BORequest { connection = new ConnectionParams { Profile = profile } };
				applyProfileFromURI(boReq, profile, rawXml, xmlSchema);
				checkBoReqParameter(boReq);
				return SAPB1.BORequest(q:boReq,name:name,id:id,result:new BOResult(Request));
			} catch (Exception e) {
				return handleException(e);
			}
		}

		// POST: api/BO/ProductionOrder - to create a BO
		[Route("api/BO/{name}")]
		public BOResult Post([FromBody]BORequest boReq, string name) {
			try {
				checkBoReqParameter(boReq);
				return SAPB1.BORequest(q:boReq, name:name, id:null, post:true, result:new BOResult(Request));
			} catch (Exception e) {
				return handleException(e);
			}
		}

		// PUT: api/BO/ProductionOrder/6756 - to update
		[Route("api/BO/{name}/{id}/{profile?}")]
		public BOResult Put([FromBody]BORequest boReq, string name, string id, string profile = null) {
			try {
				checkBoReqParameter(boReq);
				applyProfileFromURI(boReq, profile);
				return SAPB1.BORequest(q: boReq, name: name, id: id, put: true, result:new BOResult(Request));
			} catch (Exception e) {
				return handleException(e);
			}
		}

		// DELETE: api/BO/Activity/5
		[Route("api/BO/{name}/{id}/{profile?}")]
		public BOResult Delete(/*BORequest boReq,*/ string name, string id, string profile = null) {
			try {
				//checkBoReqParameter(boReq);
				BORequest boReq = new BORequest { connection = new ConnectionParams { Profile = profile } };
				applyProfileFromURI(boReq, profile);
				return SAPB1.BORequest(q: boReq, name: name, id: id, delete: true, result:new BOResult(Request));
			} catch (Exception e) {
				return handleException(e);
			}
		}
	}
}
