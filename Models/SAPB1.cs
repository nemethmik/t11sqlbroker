using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace t11sqlbroker.Models {
	public static class SAPB1 {
		static SQLBrokerConfig brokerConf = SQLBrokerConfig.GetBrokerConfig();
		static ConnectionParams getEffectiveConnectionParams(ConnectionParams fromRequest, ConnectionParams resultConnectionReference) {
			ConnectionParams cp = brokerConf.defaultConnection;
			if (!string.IsNullOrEmpty(fromRequest?.CompanyDB)) { // Request contains connection params
				if (brokerConf.connectionConfigFromCaller) {
					cp = fromRequest;
				} else {
					throw new Exception("SQL Broker is configured to reject connection parameters from clients. Only Profile name is allowed.");
				}
			}
			if (!string.IsNullOrEmpty(fromRequest?.Profile)) { // Request can contain connection profile and it overrides everything else
				ConnectionParams pcp = ConnectionParams.GetConnectionProfile(fromRequest?.Profile);
				if (pcp != null) {
					cp = pcp;
					//With the following logic, it is possible that the system admin intentionally leaves out user names and passords,
					//and the the application programmer has to take measures to make a form where the user can enter these users names
					//and corresponding passords. 
					//The simplest way is just to leave out the SAP B1 password. Of course, don't use manager as the preconfigured
					//SAP user then.
					if (string.IsNullOrEmpty(cp.Password) && !string.IsNullOrEmpty(fromRequest.Password)) cp.Password = fromRequest.Password;
					if (string.IsNullOrEmpty(cp.UserName) && !string.IsNullOrEmpty(fromRequest.UserName)) cp.UserName = fromRequest.UserName;
					if (string.IsNullOrEmpty(cp.DbUserName) && !string.IsNullOrEmpty(fromRequest.DbUserName)) cp.DbUserName = fromRequest.DbUserName;
					if (string.IsNullOrEmpty(cp.DbPassword) && !string.IsNullOrEmpty(fromRequest.DbPassword)) cp.DbPassword = fromRequest.DbPassword	;
				}
			}
			if (cp == null) throw new Exception("No connection parameters are configured or sent by client");
			else { //These are the effective connection parameters for reference to the caller
				resultConnectionReference.CompanyDB = cp.CompanyDB;
				resultConnectionReference.DbUserName = cp.DbUserName;
				resultConnectionReference.UserName = cp.UserName;
				resultConnectionReference.Server = cp.Server;
				resultConnectionReference.Profile = cp.Profile;
				resultConnectionReference.AdoNetUser = cp.AdoNetUser;
			}
			return cp;
		}
		public static SQLResult SQLQuery(SQLQuery q, bool fromGet) {
			if (!brokerConf.sql) throw new Exception("SQL module was disabled in web.config for SQL Broker");
			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			sw.Start();
			SQLResult result = new SQLResult();
			result.rollbackOnly = brokerConf.readOnly;
			if (!result.rollbackOnly) {
				result.rollbackOnly = brokerConf.readTransactionsOnSQLGet && fromGet;
			}
			if (result.rollbackOnly) {
				var sqlText = q.SQL.ToLower();
				if (sqlText.Contains("transaction") || sqlText.Contains("commit")
					|| sqlText.Contains("insert") || sqlText.Contains("update") || sqlText.Contains("delete")) {
					throw new Exception("The server entirely or the GET request is configured for read-only transactions, and your"
						+ " SQL contains the word(s): transaction, commit, insert, delete, update."
						);
					//This is the only measure we are doing for now, enforcing rollback only transaction is easy but an unnecessary extra step,
					//To guarantee read-only access simply define a DB user/login for the profile that has no db_datawriter membership
					//on the Company DB. Remember it must have db_datareader on both SBO-COMMON and CompanyDB.
				}
			}
			var cp = getEffectiveConnectionParams(q?.connection, result.connection);
			if (brokerConf.adonet) SQLADONETQuery(q, result, cp, result.rollbackOnly);
			else SQLDIQuery(q, result, cp);
			sw.Stop();
			result.execMillis = (int)sw.Elapsed.TotalMilliseconds;
			return result;
		}
		static void SQLADONETQuery(SQLQuery q, SQLResult result, ConnectionParams cp, bool rollbackOnly) {
			using (SqlConnection t = new SqlConnection(cp.getConnectionString())) //{ t.Open();
			using (SqlCommand sqlCommand = new SqlCommand(q.SQL, t)) {
				//try {
				sqlCommand.Connection.Open();
				//When the server is configured to enforce read only access
				//Simply, use a DB User that has no db_datawriter role membership.
				//if (rollbackOnly) sqlCommand.Transaction = t.BeginTransaction();
				if (q.timeOut > 0) sqlCommand.CommandTimeout = q.timeOut;
				using (SqlDataReader rs = sqlCommand.ExecuteReader(System.Data.CommandBehavior.Default)) {
					result.statusCode = System.Net.HttpStatusCode.OK;
					var data = new Newtonsoft.Json.Linq.JArray();
					do {
						while (rs.Read()) {
							var row = new Newtonsoft.Json.Linq.JObject();
							for (int i = 0; i < rs.FieldCount; i++) {
								row.Add(rs.GetName(i), rs.GetValue(i).ToString());
							}
							data.Add(row);
						}
						result.data = data;
						if (q.columnInfo) {
							int cc = rs.FieldCount;
							for (int i = 0; i < cc; i++) {
								SQLResult.Column column = new SQLResult.Column();
								column.name = rs.GetName(i);
								column.dataType = rs.GetDataTypeName(i);
								//column.subType = ;//Hmm, What to give here?
								result.columns.Add(column);
							}
						}
					} while (rs.NextResult());
					//}
					//					} finally {
					// No need for this, it's much better and dead easy to revoke the user db_datawriter membership
					//if (sqlCommand.Transaction != null) {
					//	if (rollbackOnly) sqlCommand.Transaction.Rollback();
					//	else sqlCommand.Transaction.Commit();
					//}
				}
			}
			//		t.Close();
			//		}
		}

		static void SQLDIQuery(SQLQuery q, SQLResult result, ConnectionParams cp) {
			using (var t = DIConnection.startTransaction(cp)) { //Must be used with using !!!
				SAPbobsCOM.Recordset rs = t.company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
				rs.DoQuery(q.SQL);
				result.statusCode = System.Net.HttpStatusCode.OK;
				//These mustn't be called since we get a transaction error
				//result.errorCode = t.company.GetLastErrorCode();
				//result.errorText = t.company.GetLastErrorDescription();
				string xmlText = rs.GetAsXML();
				if (q.rawXml) result.rawXml = xmlText;
				System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
				xmlDoc.LoadXml(xmlText);
				string jsonText = Newtonsoft.Json.JsonConvert.SerializeXmlNode(xmlDoc, Newtonsoft.Json.Formatting.Indented, false);
				result.data = Newtonsoft.Json.Linq.JToken.Parse(jsonText);
				if (q.columnInfo) {
					int cc = rs.Fields.Count;
					SAPbobsCOM.Fields fields = rs.Fields;
					for (int i = 0; i < cc; i++) {
						SAPbobsCOM.Field f = fields.Item(i);
						SQLResult.Column column = new SQLResult.Column();
						column.name = f.Name;
						column.dataType = f.Type.ToString();
						column.subType = f.SubType.ToString();
						//column.description = f.Description;
						SAPbobsCOM.ValidValues vvs = f.ValidValues;
						int vvc = vvs.Count;
						for (int k = 0; k < vvc; k++) {
							SAPbobsCOM.ValidValue v = vvs.Item(k);
							column.validValues.Add(new SQLResult.ValidValue { value = v.Value, description = v.Description });
						}
						result.columns.Add(column);
					}
				}
			}
		}
		/// <summary>
		/// The generic business logic behind SAP DI BO requests: GET, POST/Add, PUT/Update, DELETE/Delete
		/// </summary>
		/// <param name="q"></param>
		/// <param name="name"></param>
		/// <param name="id"></param>
		/// <param name="delete"></param>
		/// <param name="put"></param>
		/// <param name="post"></param>
		/// <returns></returns>
		public static BOResult BORequest(BORequest q, string name, string id, bool delete = false, bool put = false, bool post = false) {
			if (!brokerConf.bo) throw new Exception("SAP B1 BO module was disabled in web.config for SQL Broker");
			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			sw.Start();
			BOResult result = new BOResult();
			var cp = getEffectiveConnectionParams(q?.connection, result.connection);
			using (var t = DIConnection.startTransaction(cp)) { //Must be used with using !!!
				string cuXml = q.boXml;
				if (string.IsNullOrEmpty(cuXml)) {
					if (q.bo != null) {
						System.Xml.XmlDocument xmlDoc = Newtonsoft.Json.JsonConvert.DeserializeXmlNode(q.bo.ToString());
						cuXml = xmlDoc.OuterXml; // Maybe xmlDoc.ToString() would be OK, too
					}
				}
				result.found = true;
				string xmlText = crudBO(t, name, ref id, cuXml, delete, put, post, q.xmlSchema, ref result.xmlSchema, ref result.found);
				result.id = id;//For newly created objects the BO id is returned
				result.statusCode = System.Net.HttpStatusCode.OK;
				if (string.IsNullOrEmpty(xmlText)) {
					if (!result.found) {
						result.statusCode = System.Net.HttpStatusCode.NotFound;
						result.errorCode = (int)System.Net.HttpStatusCode.NotFound;
						result.errorText = $"Not found {name} for ID {id}";
					} else {
						if (delete) result.statusCode = System.Net.HttpStatusCode.Gone;
					}
				} else {
					if (post) result.statusCode = System.Net.HttpStatusCode.Created;
					//Is there a way to find out, when PUT/Update was requested, if nodified or not?
					//Possibly the Not Modified HTTP is a situation when the update was rejected because of some reasons.
					//if (put) result.statusCode = System.Net.HttpStatusCode.NotModified;
					if (q.rawXml) result.rawXml = xmlText;
					System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
					xmlDoc.LoadXml(xmlText);
					string jsonText = Newtonsoft.Json.JsonConvert.SerializeXmlNode(xmlDoc, Newtonsoft.Json.Formatting.Indented, false);
					result.bo = Newtonsoft.Json.Linq.JToken.Parse(jsonText);
				}
				sw.Stop();
				result.execMillis = (int)sw.Elapsed.TotalMilliseconds;
				return result;
			}
		}
		static string crudBO(DIConnection.IConnRef t, string name, ref string id, string bstrXML, 
			bool delete, bool put, bool post, bool schemaRequired, ref string xmlSchema, ref bool found) {
			if (post) { //Add new activity
				if (string.IsNullOrEmpty(bstrXML)) throw new Exception($"No XML was defined for a POST {name} request");
				if (!string.IsNullOrEmpty(id)) throw new Exception($"ID {id} was defined for a POST {name} request. How come?");
			} else if (delete) {
				if (!string.IsNullOrEmpty(bstrXML)) throw new Exception($"boXML or bo (JSON) was defined for a DELETE {name} request. Remove them from the request body and repeat the request");
				if (string.IsNullOrEmpty(id)) throw new Exception($"No ID was defined for a DELETE {name} request.");
			} else if (put) {
				if (string.IsNullOrEmpty(bstrXML)) throw new Exception($"No XML was defined for a PUT {name} request");
				if (string.IsNullOrEmpty(id)) throw new Exception($"No ID was defined for a PUT {name} request.");
			}
			switch (name) {
				case "ProductionOrders": {
					return gcrudBO<SAPbobsCOM.ProductionOrders>(SAPbobsCOM.BoObjectTypes.oProductionOrders,
						t, name, ref id, bstrXML, delete, put, post, schemaRequired, ref xmlSchema, ref found);
				}
				case "InventoryGenExit": {
					return gcrudBO<SAPbobsCOM.Documents>(SAPbobsCOM.BoObjectTypes.oInventoryGenExit,
						t, name, ref id, bstrXML, delete, put, post, schemaRequired, ref xmlSchema, ref found);
				}
				case "InventoryGenEntry": {
					return gcrudBO<SAPbobsCOM.Documents>(SAPbobsCOM.BoObjectTypes.oInventoryGenEntry,
						t, name, ref id, bstrXML, delete, put, post, schemaRequired, ref xmlSchema, ref found);
				}
				case "Activity": {
					SAPbobsCOM.ActivitiesService actSrv = t.company.GetCompanyService().GetBusinessService(SAPbobsCOM.ServiceTypes.ActivitiesService);
					SAPbobsCOM.ActivityParams oParams = actSrv.GetDataInterface(SAPbobsCOM.ActivitiesServiceDataInterfaces.asActivityParams);
					SAPbobsCOM.Activity oAct = null;
					if (!string.IsNullOrEmpty(id)) { // Find activity by ID
						oParams.ActivityCode = int.Parse(id);
					}
					if (post) { //Add new activity
						oAct = actSrv.GetDataInterface(SAPbobsCOM.ActivitiesServiceDataInterfaces.asActivity);
						oAct.FromXMLString(bstrXML);
						oParams = actSrv.AddActivity(oAct);
						id = oParams.ActivityCode.ToString();//Now we have the key
					}
					try {
						oAct = actSrv.GetActivity(oParams); //Even after a newly created activity the object should be found
					} catch (Exception e) {
						if (e.HResult == -2028) found = false; //ODBC not found error
						else throw; //Otherwise rethrow exception
					}
					if (oAct == null) {
						found = false;
						return null;
					} else {
						found = true;
						if (delete) {
							actSrv.DeleteActivity(oParams);
							oAct = null;
						} else if (put) {
							oAct.FromXMLString(bstrXML);
							actSrv.UpdateActivity(oAct);
							oAct = actSrv.GetActivity(oParams); //After update the object should be found again to return to the requestor
						}
						if (schemaRequired) xmlSchema = oAct?.GetXMLSchema();
						return oAct?.ToXMLString();
					}
				}
				default: throw new Exception("Unsupported BO type " + name);
			}
		}
		static string gcrudBO<T>(SAPbobsCOM.BoObjectTypes bobsType,
			DIConnection.IConnRef t, string name, ref string id, string bstrXML, bool delete, bool put, bool post, bool schemaRequired, ref string xmlSchema, ref bool found) {
			T bo = t.company.GetBusinessObject(bobsType);
			bool deleted = false;
			if (schemaRequired) xmlSchema = t.company.GetBusinessObjectXmlSchema(bobsType);
			if (!string.IsNullOrEmpty(id)) { // Find BO by ID
				found = ((dynamic)bo).GetByKey(int.Parse(id));
			}
			if (post) { //Add new BO // Company must be set to XMLasString = true, otherwise this will not work
				((dynamic)bo).Browser.ReadXml(bstrXML, 0);
				int status = ((dynamic)bo).Add();
				if (status != 0) {
					int errorCode = t.company.GetLastErrorCode();
					string errorText = t.company.GetLastErrorDescription();
					throw new Exception($"Add status is {status} error code {errorCode} {errorText}");
				} else {
					//Unfortunately, after addition the bo is not reloaded, to find the newly created value is as follows
					string boKey = t.company.GetNewObjectKey();
					string boType = t.company.GetNewObjectType();
					//This checking is fine, but what can we do, if it is not 202?
					//if (boType == "202") throw new Exception("The returned object type is not 202 for a Production Order");
					found = ((dynamic)bo).GetByKey(int.Parse(boKey));
					id = boKey; //With this scenario we know the key.
				}
			} else if (delete) {
				if (found) {
					int status = ((dynamic)bo).Cancel(); //No Delete operation defined for Production Order
					if (status != 0) {
						int errorCode = t.company.GetLastErrorCode();
						string errorText = t.company.GetLastErrorDescription();
						throw new Exception($"Cancel status is {status} error code {errorCode} {errorText}");
					} else {
						//This is not possible with a template typed variable
						//bo = null;
						deleted = true;
					}
				}
			} else if (put) {
				if (found) {
					((dynamic)bo).Browser.ReadXml(bstrXML, 0);
					int status = ((dynamic)bo).Update();
					if (status != 0) {
						int errorCode = t.company.GetLastErrorCode();
						string errorText = t.company.GetLastErrorDescription();
						throw new Exception($"Update status is {status} error code {errorCode} {errorText}");
					} else {
						//It's be better to reload the data, since the update in a header may have had rippling effects
						found = ((dynamic)bo).GetByKey(int.Parse(id));
					}
				}
			}
			return (found && !deleted) ? ((dynamic)bo)?.GetAsXML() : null;
		}

	}
}
