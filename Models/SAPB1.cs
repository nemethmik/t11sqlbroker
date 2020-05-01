using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace t11sqlbroker.Models {
	public static class SAPB1 {
		public static SAPbobsCOM.ProductionOrders getProductionOrderByDocEntry(ConnectionParams cp,int docEntry) {
			using (var t = DIConnection.startTransaction(cp)) { //Must be used with using !!!
				SAPbobsCOM.ProductionOrders po = t.company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oProductionOrders);
				if (po.GetByKey(docEntry)) {
					return po;
				} else return null;
			}
		}
		public static SQLResult SQLQuery(SQLQuery q) {
			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			if(q.connection == null) { throw new Exception("Connection not defined for SQLQuery"); }
			sw.Start();
			using (var t = DIConnection.startTransaction(q.connection)) { //Must be used with using !!!
				SAPbobsCOM.Recordset rs = t.company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
				rs.DoQuery(q.SQL); 
				SQLResult result = new SQLResult();
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
				sw.Stop();
				result.execMillis = (int)sw.Elapsed.TotalMilliseconds;
				return result;
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
			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			sw.Start();
			using (var t = DIConnection.startTransaction(q.connection)) { //Must be used with using !!!
				string cuXml = q.boXml;
				if(string.IsNullOrEmpty(cuXml)) {
					if (q.bo != null) {
						System.Xml.XmlDocument xmlDoc = Newtonsoft.Json.JsonConvert.DeserializeXmlNode(q.bo.ToString());
						cuXml = xmlDoc.OuterXml; // Maybe xmlDoc.ToString() would be OK, too
					}
				}
				BOResult result = new BOResult();
				result.found = true;
				result.id = id;
				string xmlText = crudBO(t,name,id, cuXml, delete, put, post, q.xmlSchema, ref result.xmlSchema, ref result.found);
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
		static string crudBO(DIConnection.IConnRef t, string name,string id,string bstrXML,bool delete,bool put,bool post, bool schemaRequired, ref string xmlSchema,ref bool found) {
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
					SAPbobsCOM.ProductionOrders bo = t.company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oProductionOrders);
					if(schemaRequired) xmlSchema = t.company.GetBusinessObjectXmlSchema(SAPbobsCOM.BoObjectTypes.oProductionOrders);
					found = bo.GetByKey(int.Parse(id));
					return found ? bo.GetAsXML() : null;
				} 
				case "InventoryGenExit": {
					SAPbobsCOM.Documents bo = t.company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenExit);
					if (schemaRequired) xmlSchema = t.company.GetBusinessObjectXmlSchema(SAPbobsCOM.BoObjectTypes.oInventoryGenExit);
					found = bo.GetByKey(int.Parse(id));
					return found ? bo.GetAsXML() : null;
				}
				case "InventoryGenEntry": {
					SAPbobsCOM.Documents bo = t.company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenEntry);
					if (schemaRequired) xmlSchema = t.company.GetBusinessObjectXmlSchema(SAPbobsCOM.BoObjectTypes.oInventoryGenEntry);
					found = bo.GetByKey(int.Parse(id));
					return found ? bo.GetAsXML() : null;
				}
				case "Activity": {
					SAPbobsCOM.ActivitiesService actSrv = t.company.GetCompanyService().GetBusinessService(SAPbobsCOM.ServiceTypes.ActivitiesService);
					SAPbobsCOM.ActivityParams oParams = actSrv.GetDataInterface(SAPbobsCOM.ActivitiesServiceDataInterfaces.asActivityParams);
					SAPbobsCOM.Activity oAct = null;
					if (!string.IsNullOrEmpty(id)) { // Find activity by ID
						oParams.ActivityCode = int.Parse(id);
					}
					if(post) { //Add new activity
						oAct = actSrv.GetDataInterface(SAPbobsCOM.ActivitiesServiceDataInterfaces.asActivity);
						oAct.FromXMLString(bstrXML);
						oParams = actSrv.AddActivity(oAct);
					}
					try {
						oAct = actSrv.GetActivity(oParams); //Even after a newly created activity the object should be found
					} catch(Exception e) {
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
	}
}