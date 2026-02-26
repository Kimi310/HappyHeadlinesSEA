CREATE DATABASE ProfanityGlobalDB;
CREATE DATABASE ProfanityAntarcticaDB;
CREATE DATABASE ProfanityAustraliaDB;
CREATE DATABASE ProfanitySouthAmericaDB;
CREATE DATABASE ProfanityEuropeDB;
CREATE DATABASE ProfanityAsiaDB;
CREATE DATABASE ProfanityAfricaDB;
CREATE DATABASE ProfanityNorthAmericaDB;

GO
USE ProfanityGlobalDB;
CREATE TABLE Profanities (
                             Id UNIQUEIDENTIFIER PRIMARY KEY,
                             Word NVARCHAR(255) NOT NULL,
);
GO

USE ProfanityAntarticaDB;
CREATE TABLE Profanities (
                             Id UNIQUEIDENTIFIER PRIMARY KEY,
                             Word NVARCHAR(255) NOT NULL,
);
GO

USE ProfanityAustraliaDB;
CREATE TABLE Profanities (
                             Id UNIQUEIDENTIFIER PRIMARY KEY,
                             Word NVARCHAR(255) NOT NULL,
);
GO

USE ProfanitySouthAmericaDB;
CREATE TABLE Profanities (
                             Id UNIQUEIDENTIFIER PRIMARY KEY,
                             Word NVARCHAR(255) NOT NULL,
);
GO

USE ProfanityEuropeDB;
CREATE TABLE Profanities (
                             Id UNIQUEIDENTIFIER PRIMARY KEY,
                             Word NVARCHAR(255) NOT NULL,
);
GO

USE ProfanityAsiaDB;
CREATE TABLE Profanities (
                             Id UNIQUEIDENTIFIER PRIMARY KEY,
                             Word NVARCHAR(255) NOT NULL,
);
GO

USE ProfanityAfricaDB;
CREATE TABLE Profanities (
                             Id UNIQUEIDENTIFIER PRIMARY KEY,
                             Word NVARCHAR(255) NOT NULL,
);
GO

USE ProfanityNorthAmericaDB;
CREATE TABLE Profanities (
                             Id UNIQUEIDENTIFIER PRIMARY KEY,
                             Word NVARCHAR(255) NOT NULL,
);
GO