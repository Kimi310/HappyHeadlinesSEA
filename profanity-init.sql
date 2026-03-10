CREATE DATABASE ProfanityGlobalDB;

GO
USE ProfanityGlobalDB;
CREATE TABLE Profanities (
                             Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                             Word NVARCHAR(255) NOT NULL,
                             CreatedAt DATETIME2 DEFAULT GETDATE()
);
GO