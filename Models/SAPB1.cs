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
				System.Xml.XmlNodeList rows = xmlDoc.SelectNodes("//row");
				for (int i = 0; i < rows.Count; i++) {
					System.Xml.XmlNode n = rows.Item(i);
					System.Xml.XmlNodeList columns = n.ChildNodes;
					Newtonsoft.Json.Linq.JObject o = new Newtonsoft.Json.Linq.JObject();
					for (int j = 0; j < rows.Count; j++) {
						System.Xml.XmlNode c = columns.Item(j);
						if (c != null) {
							string cn = c.Name;
							string cv = c.InnerText;
							o.Add(new Newtonsoft.Json.Linq.JProperty(cn, cv));
						}
					}
					result.rows.Add(o);
				}
				//string jsonText = Newtonsoft.Json.JsonConvert.SerializeXmlNode(xmlDoc);
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
	}
}