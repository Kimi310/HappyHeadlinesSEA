CREATE DATABASE ProfanityGlobalDB;

GO
USE ProfanityGlobalDB;
CREATE TABLE Profanities (
                             Id UNIQUEIDENTIFIER PRIMARY KEY,
                             Word NVARCHAR(255) NOT NULL,
);
GO