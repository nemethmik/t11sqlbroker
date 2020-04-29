# SQL Broker on ADO.NET and Docker

This project was created with Visual Studio 2019 Community Edition
I use Postman to test and experiment:

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