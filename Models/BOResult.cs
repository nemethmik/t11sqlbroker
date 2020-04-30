using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace t11sqlbroker.Models {
	public class BOResult {
		public int errorCode;
		public string errorText;
		public int execMillis;
		public Newtonsoft.Json.Linq.JToken bo;
		public string rawXml;
	}
}