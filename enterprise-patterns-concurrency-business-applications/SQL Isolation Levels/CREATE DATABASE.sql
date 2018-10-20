USE master;  
GO  
  
IF EXISTS(SELECT * from sys.databases WHERE name='TestData')  
BEGIN  
    DROP DATABASE TestData;  
END  

--Create a new database called TestData.  
CREATE DATABASE TestData; 
GO

USE TestData;

CREATE TABLE dbo.TestItems  
   (ID int PRIMARY KEY NOT NULL,  
    TestValue int)  
GO  

INSERT dbo.TestItems (ID, TestValue)  
    VALUES (1, 0) 

GO