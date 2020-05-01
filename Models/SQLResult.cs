using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace t11sqlbroker.Models {
	public class SQLResult {
		public class Column {
			public string name;
			//public string description;
			public string dataType;
			public string subType;
			public List<ValidValue> validValues = new List<ValidValue>();
		}
		public class ValidValue {
			public string description;
			public string value;
		}
		public int errorCode;
		public string errorText;
		public int execMillis;
		public List<Column> columns = new List<Column>();
		//public Newtonsoft.Json.Linq.JArray rows = new Newtonsoft.Json.Linq.JArray();
		public Newtonsoft.Json.Linq.JToken rows;
		public Newtonsoft.Json.Linq.JToken data;
		public string rawXml;
	}
}