-- Disposable sample database only. Execute in SSMS/sqlcmd, not on a production server.
IF DB_ID(N'MeridianInsights') IS NULL EXEC(N'CREATE DATABASE MeridianInsights');
GO
USE MeridianInsights;
GO
IF OBJECT_ID(N'dbo.DimAccount',N'U') IS NULL
 CREATE TABLE dbo.DimAccount(AccountId int PRIMARY KEY,AccountName nvarchar(120) NOT NULL,Sector nvarchar(70) NOT NULL,Region nvarchar(50) NOT NULL);
IF OBJECT_ID(N'dbo.FactSales',N'U') IS NULL
 CREATE TABLE dbo.FactSales(SaleId bigint PRIMARY KEY,AccountId int NOT NULL REFERENCES dbo.DimAccount(AccountId),
 OrderDate datetime2 NOT NULL,SalesAmount decimal(18,2) NOT NULL CHECK(SalesAmount>=0),
 CostAmount decimal(18,2) NOT NULL CHECK(CostAmount>=0),
 Status varchar(20) NOT NULL CHECK(Status IN ('Open','Completed','Cancelled')));
GO
IF NOT EXISTS(SELECT 1 FROM dbo.DimAccount)
 INSERT dbo.DimAccount VALUES(1,N'Northstar Cloud',N'Technology',N'Central'),(2,N'Atlas Logistics',N'Logistics',N'North'),(3,N'Cedar Retail',N'Retail',N'West');
IF NOT EXISTS(SELECT 1 FROM dbo.FactSales)
 INSERT dbo.FactSales VALUES
 (101,1,DATEADD(day,-1,SYSUTCDATETIME()),150000,95000,'Completed'),
 (102,2,DATEADD(day,-2,SYSUTCDATETIME()),76000,42000,'Completed'),
 (103,3,DATEADD(day,-3,SYSUTCDATETIME()),29000,15000,'Completed'),
 (104,1,DATEADD(day,-5,SYSUTCDATETIME()),104000,72000,'Completed'),
 (105,2,DATEADD(day,-6,SYSUTCDATETIME()),66000,41000,'Open'),
 (106,3,DATEADD(day,-8,SYSUTCDATETIME()),82000,54000,'Completed'),
 (107,2,DATEADD(day,-10,SYSUTCDATETIME()),98000,70000,'Completed'),
 (108,3,DATEADD(day,-12,SYSUTCDATETIME()),19000,11000,'Cancelled');
GO
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.FactSales') AND name='IX_FactSales_StatusDate')
 CREATE INDEX IX_FactSales_StatusDate ON dbo.FactSales(Status,OrderDate) INCLUDE(SalesAmount,CostAmount,AccountId);
GO
