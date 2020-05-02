using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace t11sqlbroker.Models {
	/// <summary>
	/// The connection details to reference a profile or define all the parameters to build a connection to a SAP B1 system.
	/// The server parameters may prevent the client to define a connection.
	/// </summary>
	public class ConnectionParams {
		/// <summary>
		/// When a profile is defined the rest of the parameters are ignored.
		/// </summary>
		public string Profile;
		/// <summary>
		/// The SAP B1 company database name
		/// </summary>
		public string CompanyDB;
		/// <summary>
		/// The database server
		/// </summary>
		public string Server;
		/// <summary>
		/// The licen server URL including the port numbet, typically 30000
		/// </summary>
		public string LicenseServer;
		/// <summary>
		/// The SAP landscape server URL with the port number, typically 40000
		/// </summary>
		public string SLDServer;
		/// <summary>
		/// The database user
		/// </summary>
		public string DbUserName;
		/// <summary>
		/// The database user's password
		/// </summary>
		public string DbPassword;
		/// <summary>
		/// Define it as false, unless you know what you are doing.
		/// </summary>
		public bool UseTrusted;
		/// <summary>
		/// The SAP B1 user name, manager, for example
		/// </summary>
		public string UserName;
		/// <summary>
		/// The password of the SAP B1 user
		/// </summary>
		public string Password;
		/// <summary>
		/// The server type: HANA, MSSQL2016
		/// </summary>
		public string DbServerType;
		static ConnectionParams MIKISURFACE = new ConnectionParams {
			Profile = "MIKISURFACE",
			CompanyDB = "SBODemoUS",
			Server = "MIKISURFACE",
			LicenseServer = "MIKISURFACE:30000",
			SLDServer = "MIKISURFACE:40000",
			DbUserName = "sa",
			DbPassword = "B1Admin",
			UseTrusted = false,
			UserName = "manager",
			Password = "B1Admin",
			DbServerType = "MSSQL2016"
		};
		/// <summary>
		/// Returns connection parameter values from Web.config
		/// <configuration>	
		/// <configSections>
		///			<section name = "SQLBrokerDefault" type="System.Configuration.SingleTagSectionHandler"/>
		///		</configSections>
		///		<SQLBrokerDefault CompanyDB = "SBODemoUS" Server="MIKISURFACE" LicenseServer="MIKISURFACE:30000"
		///			SLDServer="MIKISURFACE:40000" DbUserName="sa" DbPassword="B1Admin" UserName="manager" Password="B1Admin"
		///			DbServerType="MSSQL2016"/>
		///		....
		/// </configuration>
		/// </summary>
		/// <param name="profile"></param>
		/// <returns></returns>
		public static ConnectionParams GetConnectionProfile(string profile) {
			if (string.IsNullOrEmpty(profile)) throw new Exception("No conf was found for " + profile);
			var conf = (System.Collections.Hashtable)System.Configuration.ConfigurationManager.GetSection(profile);
			if (conf != null) {
				var cp = new ConnectionParams {
					Profile = profile,
					CompanyDB = conf["CompanyDB"].ToString(),
					Server = conf["Server"].ToString(),
					LicenseServer = conf["LicenseServer"].ToString(),
					SLDServer = conf["SLDServer"].ToString(),
					DbUserName = conf["DbUserName"].ToString(),
					DbPassword = conf["DbPassword"].ToString(),
					UseTrusted = false,
					UserName = conf["UserName"].ToString(),
					Password = conf["Password"].ToString(),
					DbServerType = conf["DbServerType"].ToString(),
				};
				return cp;
			} else {
				throw new Exception("No conf was found for " + profile);
			}
		}
		/// <summary>
		/// Converts the STRING database type to SAPbobsCOM.BoDataServerTypes
		/// </summary>
		/// <returns></returns>
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