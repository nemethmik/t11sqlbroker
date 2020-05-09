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
				return SAPB1.BORequest(q: boReq, name: name, id: id, result: new BOResult(Request));
			} catch (Exception e) {
				return handleException(e);
			}
		}

		// POST: api/BO/ProductionOrder - to create a BO
		[Route("api/BO/{name}")]
		public BOResult Post([FromBody]BORequest boReq, string name) {
			try {
				checkBoReqParameter(boReq);
				return SAPB1.BORequest(q: boReq, name: name, id: null, post: true, result: new BOResult(Request));
			} catch (Exception e) {
				return handleException(e);
			}
		}

		// POST: api/MR?profile=TibiProfLive
		[Route("api/MR/{profile?}")]
		public MultiResult Post([FromBody]MultiRequest mr, string profile = null) {
			MultiResult result = new MultiResult(Request);
			try {
				if (mr == null) throw new Exception("The body is missing or not formatted correctly. Maybe just a comma is missing between two attributes.");
				//Apply profile from URI
				if (!string.IsNullOrEmpty(profile)) {
					if (mr.connection == null) mr.connection = new ConnectionParams { Profile = profile };
					else if (string.IsNullOrEmpty(mr.connection.Profile)) mr.connection.Profile = profile;
				}
				System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
				sw.Start();
				var cp = SAPB1.getEffectiveConnectionParams(mr?.connection, ref result.connection);
				//START Transaction with using and pass the same transaction object to each request processor
				using (var t = DIConnection.startTransaction(cp)) { //Must be used with using !!!
					MReqResult reqResult = null;
					try {
						result.totalJobsRequested = mr.requests.Count;
						for (int i = 0; i < mr.requests.Count; i++) {
							result.index = i;
							MReqJob mrJob = mr.requests[i];
							reqResult = new MReqResult();
							if (!string.IsNullOrEmpty(mrJob.boName) || mrJob.boReq != null) {
								//result.reqJob = mrJob;
								bool post = false;
								bool delete = false;
								bool put = false;
								bool getReq = false;
								if (string.IsNullOrEmpty(mrJob.reqType)) {
									throw new SQLBrokerError($"Multi-Request BO job {mrJob.boName} has no reqType property (GET, PUT, DELETE, POST)");
								} else {
									switch (mrJob.reqType) {
										case "GET": getReq = true; break;
										case "POST": post = true; break;
										case "PUT": put = true; break;
										case "DELETE": delete = true; break;
										default: {
											throw new SQLBrokerError($"Multi-Request BO job {mrJob.boName} reqType {mrJob.reqType} is invalid. Use GET, PUT, DELETE, POST");
										}
									}
								}
								if (mrJob.boReq == null) {
									if (getReq || delete) {
										mrJob.boReq = new BORequest { connection = new ConnectionParams { Profile = profile } };
									} else {
										throw new SQLBrokerError($"Multi-Request BO job {mrJob.boName} no boReq object defined for PUT, POST");
									}
								}
								if (string.IsNullOrEmpty(mrJob.boName)) {
									throw new SQLBrokerError("Multi-request BO job has no boName property");
								}
								if (string.IsNullOrEmpty(mrJob.boId)) {
									if (getReq || put || delete) {
										throw new SQLBrokerError($"Multi-Request BO job {mrJob.boName} reqType {mrJob.reqType} requires non-empty boId property.");
									}
								} else {
									if (post) {
										throw new SQLBrokerError($"Multi-Request BO job {mrJob.boName} reqType {mrJob.reqType} doesn't allow boId property. Delete boId from your request.");
									}
								}
								reqResult.boResult = new BOResult(Request);
								reqResult.boResult.connection = new NoPwdConnectionParams(cp);
								reqResult.boResult.jobNumber = i;
								SAPB1.BOJob(t, q: mrJob.boReq, name: mrJob.boName, id: mrJob.boId, post: post, delete: delete, put: put, result: reqResult.boResult);
							}
							if (mrJob.sqlReq != null) {
								throw new SQLBrokerError($"Multi-Request SQL is not supported currently. Stay tuned, however.");
								//var sqlResult = SAPB1.SQLQuery(value, false, new SQLResult(Request));
							}
							result.results.Add(reqResult);
						}
						t.Commit();
						sw.Stop();
						result.execMillis = (int)sw.Elapsed.TotalMilliseconds;
						return result;
					} catch (Exception) {
						result.errorResult = reqResult;
						//ROLLBACK Transaction, if not rolled back alreadt
						t.Rollback();
						throw;
					}
				}
				//COMMIT the transacyion if no errors occured
			} catch (Exception e) {
				if (e is SQLBrokerError) {
					SQLBrokerError brokerError = e as SQLBrokerError;
					if (result.errorResult == null) result.errorResult = new MReqResult();
					result.errorResult.boResult = brokerError.boResult;
					result.errorResult.sqlResult = brokerError.sqlResult;
				}
				return result.setResponseStatus(HttpStatusCode.BadRequest, e);
			}
		}


		// PUT: api/BO/ProductionOrder/6756 - to update
		[Route("api/BO/{name}/{id}/{profile?}")]
		public BOResult Put([FromBody]BORequest boReq, string name, string id, string profile = null) {
			try {
				checkBoReqParameter(boReq);
				applyProfileFromURI(boReq, profile);
				return SAPB1.BORequest(q: boReq, name: name, id: id, put: true, result: new BOResult(Request));
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
				return SAPB1.BORequest(q: boReq, name: name, id: id, delete: true, result: new BOResult(Request));
			} catch (Exception e) {
				return handleException(e);
			}
		}
	}
}
