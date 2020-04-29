using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace t11sqlbroker.Models {
	public class ConnectionParams {
		public string CompanyDB;
		public string Server;
		public string LicenseServer;
		public string SLDServer;
		public string DbUserName;
		public string DbPassword;
		public bool UseTrusted;
		public string UserName;
		public string Password;
		public string DbServerType;
		public static ConnectionParams MIKISURFACE = new ConnectionParams {
			CompanyDB = "SBODemoUS",
			Server = "MIKISURFACE",
			LicenseServer = "MIKISURFACE:30000",
			SLDServer = "MIKISURFACE:40000",
			DbUserName = "sa",
			DbPassword = "B1Admin",
			UseTrusted = true,
			UserName = "manager",
			Password = "B1Admin",
			DbServerType = "MSSQL2016"
		};
		public SAPbobsCOM.BoDataServerTypes boDataServerType() {
			SAPbobsCOM.BoDataServerTypes st = SAPbobsCOM.BoDataServerTypes.dst_HANADB;
			if(this.DbServerType.Equals("MSSQL2017")) st = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2017;
			else if (this.DbServerType.Equals("MSSQL2016")) st = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2016;
			else if (this.DbServerType.Equals("MSSQL2014")) st = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2014;
			else if (this.DbServerType.Equals("MSSQL2012")) st = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2012;
			else if (this.DbServerType.Equals("MSSQL2008")) st = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2008;
			else if (this.DbServerType.Equals("MSSQL2005")) st = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2005;
			return st;
		}
	}
}