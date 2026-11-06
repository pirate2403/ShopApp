-- Запуск в SSMS с правами создания БД. Данные при повторном запуске сохраняются.
USE master;
GO
IF DB_ID(N'ShopLab') IS NULL CREATE DATABASE ShopLab;
GO
USE ShopLab;
GO
IF OBJECT_ID(N'dbo.Purchases',N'U') IS NULL
CREATE TABLE dbo.Purchases (
 Id int IDENTITY(1,1) PRIMARY KEY,
 Customer nvarchar(80) NOT NULL CHECK(LEN(LTRIM(RTRIM(Customer)))>0),
 Product nvarchar(100) NOT NULL CHECK(LEN(Product)>0),
 Category nvarchar(40) NOT NULL CHECK(LEN(Category)>0),
 Price decimal(12,2) NOT NULL CHECK(Price>0),
 Gift bit NOT NULL, Warranty bit NOT NULL, Insurance bit NOT NULL, Courier bit NOT NULL,
 CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO
CREATE OR ALTER FUNCTION dbo.PurchaseTotal(@Price decimal(12,2),@Gift bit,@Warranty bit,@Insurance bit,@Courier bit)
RETURNS decimal(12,2) AS
BEGIN
 RETURN @Price + CASE WHEN @Gift=1 THEN 200 ELSE 0 END
 + CASE WHEN @Warranty=1 THEN 1000 ELSE 0 END
 + CASE WHEN @Insurance=1 THEN ROUND(@Price*0.03,2) ELSE 0 END
 + CASE WHEN @Courier=1 THEN 350 ELSE 0 END;
END;
GO
CREATE OR ALTER VIEW dbo.PurchaseSummary AS
SELECT Id,Customer,Product,Category,Price,Gift,Warranty,Insurance,Courier,
 dbo.PurchaseTotal(Price,Gift,Warranty,Insurance,Courier) AS Total,CreatedAt
FROM dbo.Purchases;
GO
CREATE OR ALTER PROCEDURE dbo.Purchase_List AS
BEGIN
 SET NOCOUNT ON;
 SELECT Id,Customer,Product,Category,Price,Gift,Warranty,Insurance,Courier,Total
 FROM dbo.PurchaseSummary ORDER BY Id DESC;
END;
GO
CREATE OR ALTER PROCEDURE dbo.Purchase_Add
 @Customer nvarchar(80),@Product nvarchar(100),@Category nvarchar(40),@Price decimal(12,2),
 @Gift bit,@Warranty bit,@Insurance bit,@Courier bit
AS
BEGIN
 SET NOCOUNT ON;
 INSERT dbo.Purchases(Customer,Product,Category,Price,Gift,Warranty,Insurance,Courier)
 VALUES(@Customer,@Product,@Category,@Price,@Gift,@Warranty,@Insurance,@Courier);
END;
GO
CREATE OR ALTER PROCEDURE dbo.Purchase_Update
 @Id int,@Customer nvarchar(80),@Product nvarchar(100),@Category nvarchar(40),@Price decimal(12,2),
 @Gift bit,@Warranty bit,@Insurance bit,@Courier bit
AS
BEGIN
 SET NOCOUNT ON;
 UPDATE dbo.Purchases SET Customer=@Customer,Product=@Product,Category=@Category,Price=@Price,
 Gift=@Gift,Warranty=@Warranty,Insurance=@Insurance,Courier=@Courier WHERE Id=@Id;
 IF @@ROWCOUNT=0 THROW 50001,N'Покупка уже удалена. Обновите список.',1;
END;
GO
CREATE OR ALTER PROCEDURE dbo.Purchase_Delete @Id int AS
BEGIN
 SET NOCOUNT ON;
 DELETE dbo.Purchases WHERE Id=@Id;
 IF @@ROWCOUNT=0 THROW 50001,N'Покупка уже удалена. Обновите список.',1;
END;
GO
IF DATABASE_PRINCIPAL_ID(N'ShopOperator') IS NULL CREATE ROLE ShopOperator;
GO
GRANT EXECUTE ON dbo.Purchase_List TO ShopOperator;
GRANT EXECUTE ON dbo.Purchase_Add TO ShopOperator;
GRANT EXECUTE ON dbo.Purchase_Update TO ShopOperator;
GRANT EXECUTE ON dbo.Purchase_Delete TO ShopOperator;
GRANT SELECT ON dbo.PurchaseSummary TO ShopOperator;
-- Пример для существующей учётной записи Windows; замените COMPUTER\Student:
-- USE master;
-- CREATE LOGIN [COMPUTER\Student] FROM WINDOWS;
-- USE ShopLab;
-- CREATE USER [COMPUTER\Student] FOR LOGIN [COMPUTER\Student];
-- ALTER ROLE ShopOperator ADD MEMBER [COMPUTER\Student];
-- Не выдавайте пользователю db_owner, если хотите проверить ограничения роли.
GO
