# SQL Broker on ADO.NET and Docker

## Motivation
This is a terribly powerful and flexible server to execute any SQL operations against an SQL database as well as support specifically SAP Business One User Queries and BUsiness Objects via the SAP B1 DI API.
A number of experts would say that having a service like this is dangerous since it exposes the server's total functionality. This is totally true, but, here are why it is still meaningful.
- SAP architects themselves created a server called Service Layer (SL) for SAP B1, which is a generic ODATA service exposing the entire company database to the public. SQL Broker can be configured to support only the BO and UQ services, which gives more or less the same functinality as SL.
- GraphQL is a popular query language, a lot more complex still a lot less powerful and less mature than SQL, and when you expose your system via GraphQL you reach the same level of threats as with SQL Broker or SL.
- When working with SQL Broker it is highly recommended to enable and elaborate an appropriate authorization and authentication (AAA) system on SQL Server, SQL Server has brilliant and flexible tools for that. Companies taking data security seriously have their database experts and they can make AAA layers in SQL Server, and the user used by SQL Broker could be totally managed by this system. It's not the SA user that is to be used for applications. SQL Broker, SAP's own Service Layer itself is a motivation for the companies to apply database security measures.
- SQL Broker can be preconfigured to
	- Work with predefined connection profiles
	- BO/UQ services only
	- Read-Only Transactions

## Generic SQL Execution
This project was created with Visual Studio 2019 Community Edition
I used Postman to test and experiment:

```json
POST http://localhost:63656/Api/SQL
{
	connection: {
		CompanyDB:"SBODemoUS",
		Server:"MIKISURFACE",
		LicenseServer:"MIKISURFACE:30000",
		SLDServer:"MIKISURFACE:40000",
		DbUserName:"sa",
		DbPassword:"B1Admin",
		UseTrusted:true,
		UserName:"manager",
		Password:"B1Admin",
		DbServerType:"MSSQL2016"	
	},
	SQL:"begin
	UPDATE OITM SET FrgnName = 'Irodai nyomtató'
	where ItemCode = 'A00001'
	UPDATE OITM SET FrgnName = 'Másik Irodai nyomtató'
	where ItemCode = 'A00002'
	DELETE FROM [@XXXTEST]
	INSERT INTO [@XXXTEST] values ('0013','Test Value 13')
	INSERT INTO [@XXXTEST] values('0014','Test Value 14') 
	--DELETE FROM [@XXXTEST]
	select * from [@XXXTEST]
	-- select ItemCode, FrgnName from OITM where ItemCode in ('A00001','A00002','A00003')
	-- Only the first query result is returned by DI.Resultset 
	--select ItemName, ItemCode from OITM where ItemCode in ('A00004','A00005','A00006')
end
",
	maxRows: 1000,
	timeOut:10,
	rawXml:true,
	columnInfo:true,
}
```

Both GET and POST use exactly the same protocol. 
The general idea is that, if the SQL statement block contains at least one UPDATE/INSERT/DELETE or a procedure call that modifies the database, use POST.
SQL Broker supports SQL execution via both DI Recordset and ADO.NET. ADO.NET supports read-only transactions, so a GET request starts a read-only transaction automatically.
The server can be pre-configured to support only a handful of profiles, in that case the connection parameters are ignored. The server can be configured to support only queries, and that case all transactions are started as read only.

## SAP DI Business Object Service
These are for 
- POST api/BO/ProductionOrders to create a new production order, 
- GET api/BO/ProductionOrders/99 to get an existing PO
- PUT api/BO/ProductionOrders/99 to update an existing PO
- DELETE api/BO/ProductionOrders/99 to delete an existing PO, actually it is not possible for POs in SAP B1, but applicable for a couple of other entities. Activities, for example, can be deleted.

Here are a couple of examples for getters. The HTTP body may contain the connection parameters and the entire request can be defined in the body, the BO name and ID included.

