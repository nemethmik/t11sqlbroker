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
				//System.Xml.XmlNodeList rows = xmlDoc.SelectNodes("//row");
				//for (int i = 0; i < rows.Count; i++) {
				//	System.Xml.XmlNode n = rows.Item(i);
				//	System.Xml.XmlNodeList columns = n.ChildNodes;
				//	Newtonsoft.Json.Linq.JObject o = new Newtonsoft.Json.Linq.JObject();
				//	for (int j = 0; j < rows.Count; j++) {
				//		System.Xml.XmlNode c = columns.Item(j);
				//		if (c != null) {
				//			string cn = c.Name;
				//			string cv = c.InnerText;
				//			o.Add(new Newtonsoft.Json.Linq.JProperty(cn, cv));
				//		}
				//	}
				//	result.rows.Add(o);
				//}
				string jsonText = Newtonsoft.Json.JsonConvert.SerializeXmlNode(xmlDoc, Newtonsoft.Json.Formatting.Indented, false);
				result.data = Newtonsoft.Json.Linq.JToken.Parse(jsonText);
				/* 
				 * In this section I tried to extract out the actual rows from the JToken object, but it would require a lot of time
				 * So, I'll keep the rows for ADO.NET implementation
				 
				var bom = result.data?.Children().ElementAt(1);
				var bo = bom?.First;
				var oclg = bo?.First().First().Children().ElementAt(1);
				var row = oclg?.Children();
				result.rows = row?.First();
				*/
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

		public static BOResult BORequest(BORequest q, string name, string id) {
			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			sw.Start();
			using (var t = DIConnection.startTransaction(q.connection)) { //Must be used with using !!!
				string xmlText = getBOXml(t,name,id);
				BOResult result = new BOResult();
				if (string.IsNullOrEmpty(xmlText)) {
					result.errorCode = -600811;
					result.errorText = $"Not found {name} for ID {id}";
				} else {
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
		static string getBOXml(DIConnection.IConnRef t, string name,string id) {
			switch (name) {
				case "ProductionOrders": {
					SAPbobsCOM.ProductionOrders bo = t.company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oProductionOrders);
					return bo.GetByKey(int.Parse(id)) ? bo.GetAsXML() : null;
				} 
				case "InventoryGenExit": {
					SAPbobsCOM.Documents bo = t.company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenExit);
					return bo.GetByKey(int.Parse(id)) ? bo.GetAsXML() : null;
				}
				case "InventoryGenEntry": {
					SAPbobsCOM.Documents bo = t.company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenEntry);
					return bo.GetByKey(int.Parse(id)) ? bo.GetAsXML() : null;
				}
				case "Activity": {
					SAPbobsCOM.ActivitiesService oActSrv = t.company.GetCompanyService().GetBusinessService(SAPbobsCOM.ServiceTypes.ActivitiesService);
					SAPbobsCOM.ActivityParams oParams = oActSrv.GetDataInterface(SAPbobsCOM.ActivitiesServiceDataInterfaces.asActivityParams);
					oParams.ActivityCode = int.Parse(id);
					SAPbobsCOM.Activity oAct = oActSrv.GetActivity(oParams);
					return oAct != null ? oAct.ToXMLString() : null;
				}
				default: throw new Exception("Unsupported BO type " + name);
			}
		}
	}
}