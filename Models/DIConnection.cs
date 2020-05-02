using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace t11sqlbroker.Models {
	/// <summary>
	/// Corresponds to SAPbobsCOM Company
	/// </summary>
	public class DIConnection : IDisposable {
		public interface IConnRef : IDisposable {
			SAPbobsCOM.Company company { get;}
			void Rollback();
			void Commit();
		}
		/// <summary>
		/// This represents a transaction, actually
		/// </summary>
		class ConnRef : IConnRef {
			SAPbobsCOM.Company IConnRef.company { get {
					if (DIConnection.currentRef == null) throw new Exception("Conn not initialized");
					else {
						//The transaction should be started when the ConnRef object is created
						//Me.company.StartTransaction();
						return Me.company;
					}
				}
			}
			public void Rollback() { Me.Rollback(); }
			public void Commit() { Me.Commit(); }
			public ConnRef(DIConnection conn) {
				if (DIConnection.currentRef != null) throw new Exception("Connection is in use");
				else {
					Me.company.StartTransaction();
					DIConnection.currentRef = this;
				}
			}
			public void Dispose() {
				DIConnection.currentRef = null;
				Me.Commit();
			}
		}
		public SAPbobsCOM.Company company => _company;
		private SAPbobsCOM.Company _company;
		ConnectionParams connectionParams;
		private DIConnection() {}
		public static IConnRef startTransaction(ConnectionParams cp) {
			Me.Connect(cp);
			return new ConnRef(Me);
		}
		private static ConnRef currentRef;
		public static DIConnection Me = new DIConnection();
		public void Rollback() {
			if (company != null && company.Connected && company.InTransaction) {
				company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);
			}
		}
		public void Commit() {
			if (company != null && company.Connected && company.InTransaction) {
				company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
			}
		}
		/// <summary>
		/// Dispose guarantees that the DI API link is disposed correctly by ASP.NET runtime.
		/// No need for explicit disconnection. The connection is automatically disconnected.
		/// </summary>
		public void Dispose() {
			Disconnect();
			currentRef = null;
		}
		/// <summary>
		/// Builds a connection to SAP B1 via DI
		/// </summary>
		/// <returns></returns>
		public void Connect(ConnectionParams connectionParams) {
			this.connectionParams = connectionParams;
			if (this._company == null) {
				this._company = new SAPbobsCOM.Company();
				_company.CompanyDB = connectionParams.CompanyDB;
				_company.Server = connectionParams.Server;
				_company.LicenseServer = connectionParams.LicenseServer;
				_company.SLDServer = connectionParams.SLDServer;
				_company.DbUserName = connectionParams.DbUserName;
				_company.DbPassword = connectionParams.DbPassword;
				//UseTrusted must be false when DB User is defined, otherwise cannot connect to SBO-COMMON error is thrown on IIS
				_company.UseTrusted = string.IsNullOrEmpty(connectionParams.DbUserName) ? connectionParams.UseTrusted : false;
				_company.UserName = connectionParams.UserName;
				_company.Password = connectionParams.Password;
				_company.DbServerType = connectionParams.boDataServerType();
				int status = _company.Connect();
				_company.XMLAsString = true; //THIS IS TERRIBLY IMPORTANT for XML handling
				System.Diagnostics.Debug.WriteLine("SAP DI is connected");
			}
			if (!this._company.Connected) {
				string errorMsg = _company.GetLastErrorDescription();
				int errorCode = _company.GetLastErrorCode();
				throw new Exception($"Connection was rejected msg { errorMsg } code { errorCode}");
			}
		}
		void Disconnect() {
			if (_company != null && _company.Connected) {
				_company.Disconnect();
				_company = null;
				System.Diagnostics.Debug.WriteLine("SAP DI is disconnected");
			}
		}
	}
}