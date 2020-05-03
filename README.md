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
```json
GET http://MIKISURFACE/t11sqlbroker/Api/BO/ProductionOrders/1
{ 
	connection: { 
		CompanyDB:"SBODemoUS", 
		Server:"MIKISURFACE", 
		LicenseServer:"MIKISURFACE:30000", 
		SLDServer:"MIKISURFACE:40000", 
		DbUserName:"sa", 
		DbPassword:"B1Admin", 
		UseTrusted:false, 
		UserName:"manager", 
		Password:"B1Admin", 
		DbServerType:"MSSQL2016" 
		
	}, 
	timeOut:10, 
	rawXml:true, 
}
```
The response is something like:
```json
{
    "errorCode": 0,
    "errorText": null,
    "execMillis": 156,
    "bo": {
        "?xml": {
            "@version": "1.0",
            "@encoding": "UTF-16"
        },
        "BOM": {
            "BO": {
                "AdmInfo": {
                    "Object": "202"
                },
                "OWOR": {
                    "row": {
                        "DocEntry": "1",
                        "DocNum": "1",
                        "Series": "23",
                        "ItemCode": "P10001",
                        "Status": "L",
                        "Type": "S",
                        "PlannedQty": "20.000000",
                        "CmpltQty": "20.000000",
                        "RjctQty": "0.000000",
                        "PostDate": "20060110",
                        "DueDate": "20060115",

                      }
                }
            }
        }
    },
    "rawXml": "<?xml version=\"1.0\" encoding=\"UTF-16\"?><BOM><BO><AdmInfo><Object>202</Object></AdmInfo><OWOR><row><DocEntry>1</DocEntry><DocNum>1</DocNum><Series>23</Series><ItemCode>P10001</ItemCode><Status>L</Status><Type>S</Type><PlannedQty>20.000000</PlannedQty><CmpltQty>20.000000</CmpltQty><RjctQty>0.000000</RjctQty><PostDate>20060110</PostDate><DueDate>20060115</DueDate><OriginAbs nil=\"true\">0</ ...
    RtCalcProp><Status>P</Status><ItemName>Labor Hours Production</ItemName></row></WOR1></BO></BOM>"
}
```
- POST http://MIKISURFACE/t11sqlbroker/Api/BO/ProductionOrders
```json
{ 
	comment:"POST This is a Production Order creation",
	connection: { 
		Profile:"MikiTest",
	}, 
	bo: {
		"BOM": {
		  "BO": {
		      "AdmInfo": {
		          "Object": "202"
		      },
		      "OWOR": {
		          "row": {
		              "Series": "23",
		              "ItemCode": "P20003",
		              "Type": "S",
		              "PlannedQty": "23.000000",
		              "DueDate": "20200916",
		              "Warehouse": "01",
		              "StartDate": "20200810",
		              "Priority": "100"
		          }
		      },
		  }
		}
	},
	timeOut:10, 
	rawXml:false, 
}
```



- http://MIKISURFACE/t11sqlbroker/Api/BO/Activity/1 to get/put/delete
```json
POST http://MIKISURFACE/t11sqlbroker/Api/BO/Activity
{ 
	connection: { 
		CompanyDB:"SBODemoUS", 
		Server:"MIKISURFACE", 
		LicenseServer:"MIKISURFACE:30000", 
		SLDServer:"MIKISURFACE:40000", 
		DbUserName:"sa", 
		DbPassword:"B1Admin", 
		UseTrusted:false, 
		UserName:"manager", 
		Password:"B1Admin", 
		DbServerType:"MSSQL2016" 
	}, 
	bo: {
		Activity: {
		    CardCode: "C20000",
		    Notes: "MMMMMM",
		    StartDate: "2020-07-25",
		    Details: "MEETING NOW",
		    Activity: "cn_Conversation",
		    ActivityType: "-1",
		    StartTime: "02:44:00",
		    EndTime: "02:59:00",
		}
	    },
	timeOut:10, 
	rawXml:false, 
	xmlSchema: false,
}

```

- http://MIKISURFACE/t11sqlbroker/Api/BO/InventoryGenEntry/1
```json
{
  comment:"This is Receive from Production example", 
  connection: { 
    Profile:"MikiTest",
  }, 
  "bo": {
  "BOM": {
      "BO": {
          "AdmInfo": {
              "Object": "59"
          },
          "OIGN": {
              "row": {
                  "Comments": "ZZZZZZZZZZZZ",
                  "JrnlMemo": "Receipt from Production from SQL Broker",
              }
          },
          "IGN1": {
              "row": {
                  "BaseRef": "162",
                  "BaseType": "202",
                  "BaseEntry": "162",
                  "Quantity": "1.000000",
                  "WhsCode": "01"
              }
          }
      }
  }
  },
  timeOut:10, 
  rawXml:false, 
}
```

- http://MIKISURFACE/t11sqlbroker/Api/BO/InventoryGenExit/11
```json
{ 
	comment:"POST to create an Issue for Production Document",
	connection: { 
		Profile:"MikiTest",
	}, 
	  "bo": {
      "BOM": {
          "BO": {
              "AdmInfo": {
                  "Object": "60"
              },
              "OIGE": {
                  "row": {
                      "Ref1": "2",
                      "Comments": "YYYYYYYYYYYYYYYYY",
                      "JrnlMemo": "Issue for Production"
                  }
              },
              "IGE1": {
                  "row": [
                      {
                          "BaseRef": "162",
                          "BaseType": "202",
                          "BaseEntry": "162",
                          "BaseLine": "0",
                          "Quantity": "1.000000",
                          "WhsCode": "01"
                      },
                      {
                          "BaseRef": "162",
                          "BaseType": "202",
                          "BaseEntry": "162",
                          "BaseLine": "3",
                          "Quantity": "1.000000",
                          "WhsCode": "01"
                      }
                  ]
              }
          }
      }
  },
	timeOut:10, 
	rawXml:false, 
}
```

## Security Configurations for SQL Broker
Here are the steps to make a DB login/user for SQL Broker:
- Create a Login account (SQLBroker, for example) on the Serer's Security/Login folder, just leave it only with public server role
- Create a user on the company database's Security/Users folder, and create a user (SQLBroker, for example) and assign it to the login account SQLBroker
- Create new role db_executor (or whatever) on the Database Roles folder.
	- Open a query window and enter **GRANT EXECUTE to db_executor**
	- Open the Properties window for the Database User SQLBroker and on the membership panel select all these three
		- db_datareader
		- db_datawriter
		- db_executor (this is what we have just created)
- Create a user on SBO-COMMON database Security/Users folder, and create a user (SQLBroker, for example) and assign it to the login account SQLBroker
	- Open the Properties window for the Database User SQLBroker in SBO-COMMON and on the membership panel select only the
		- db_datareader
Now, this SQLBroker, or whatever name you used, can be used as a DB user for SAP B1, no need to use SA any more.

### Execution Permissions
When you receive the error message:
"[Microsoft][ODBC Driver 13 for SQL Server][SQL Server]The EXECUTE permission was denied on the object 'SBO_SP_TransactionNotification', database 'SBODemoUS', schema 'dbo'. (CINF)",
This means that the DB user has no execution authorization on the Company DB.
How to enable it without giving full **db_owner** SQL Server membership to the DB user defined for the connection profile?
```SQL
-- Open console for the database
CREATE ROLE db_executor --This can be done with the Studio UI, too
GRANT EXECUTE to db_executor -- This can only be done on the console/script window
ALTER ROLE db_executor ADD MEMBER SQLBroker -- This can be done with Studio UI, too.
-- These are to query some useful information
EXEC sp_helprotect @name='EXECUTE' -- Returns db_executor
SELECT is_rolemember('db_executor','SQLBroker')
```

### Login Issues
When you receive a message this usually means that the password or the user of the DbUserName/DbPassord parameters are not ok.
```json
{
    "statusCode": 400,
    "errorText": "Connection was rejected msg Unable to access SBO-Common database code -111",
}
```
Check the web.config settings and make sure your passwords are ok.
