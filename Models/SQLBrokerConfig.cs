using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace t11sqlbroker.Models {
	public class SQLBrokerConfig {
		public bool connectionConfigFromCaller = true;
		public bool readOnly = false;
		public bool readTransactionsOnSQLGet = true;
		public bool sql = true;
		public bool bo = true;
		public bool uq = true;
		public bool adonet = true;
		public string defaultProfile;
		public int maxConnections = 3;
		public ConnectionParams defaultConnection;
		public static SQLBrokerConfig GetBrokerConfig() {
			var conf = (System.Collections.Hashtable)System.Configuration.ConfigurationManager.GetSection("SQLBrokerConfig");
			if (conf != null) {
				var c = new SQLBrokerConfig {
					connectionConfigFromCaller = bool.Parse(conf["connectionConfigFromCaller"].ToString()),
					readOnly = bool.Parse(conf["readOnly"].ToString()),
					readTransactionsOnSQLGet = bool.Parse(conf["readTransactionsOnSQLGet"].ToString()),
					sql = bool.Parse(conf["sql"].ToString()),
					bo = bool.Parse(conf["bo"].ToString()),
					uq = bool.Parse(conf["uq"].ToString()),
					defaultProfile = conf["defaultProfile"].ToString(),
					maxConnections = int.Parse(conf["maxConnections"].ToString()),
				};
				if (!string.IsNullOrEmpty(c.defaultProfile)) {
					c.defaultConnection = ConnectionParams.GetConnectionProfile(c.defaultProfile);
				}
				return c;
			} else return new SQLBrokerConfig();
		}
	}
}