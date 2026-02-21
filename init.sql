CREATE DATABASE EuropeDB;
CREATE DATABASE AsiaDB;
CREATE DATABASE AfricaDB;
CREATE DATABASE NorthAmericaDB;
CREATE DATABASE SouthAmericaDB;
CREATE DATABASE AustraliaDB;
CREATE DATABASE AntarcticaDB;
CREATE DATABASE GlobalDB;
GO

USE GlobalDB;
CREATE TABLE Articles (
                          Id UNIQUEIDENTIFIER PRIMARY KEY,
                          Title NVARCHAR(200) NOT NULL,
                          Content NVARCHAR(MAX) NOT NULL,
                          Continent NVARCHAR(50) NOT NULL,
                          IsGlobal BIT NOT NULL
);
GO

USE AntarcticaDB;
CREATE TABLE Articles (
                          Id UNIQUEIDENTIFIER PRIMARY KEY,
                          Title NVARCHAR(200) NOT NULL,
                          Content NVARCHAR(MAX) NOT NULL,
                          Continent NVARCHAR(50) NOT NULL,
                          IsGlobal BIT NOT NULL
);
GO

USE AustraliaDB;
CREATE TABLE Articles (
                          Id UNIQUEIDENTIFIER PRIMARY KEY,
                          Title NVARCHAR(200) NOT NULL,
                          Content NVARCHAR(MAX) NOT NULL,
                          Continent NVARCHAR(50) NOT NULL,
                          IsGlobal BIT NOT NULL
);
GO

USE SouthAmericaDB;
CREATE TABLE Articles (
                          Id UNIQUEIDENTIFIER PRIMARY KEY,
                          Title NVARCHAR(200) NOT NULL,
                          Content NVARCHAR(MAX) NOT NULL,
                          Continent NVARCHAR(50) NOT NULL,
                          IsGlobal BIT NOT NULL
);
GO

USE EuropeDB;
CREATE TABLE Articles (
                          Id UNIQUEIDENTIFIER PRIMARY KEY,
                          Title NVARCHAR(200) NOT NULL,
                          Content NVARCHAR(MAX) NOT NULL,
                          Continent NVARCHAR(50) NOT NULL,
                          IsGlobal BIT NOT NULL
);
GO

USE AsiaDB;
CREATE TABLE Articles (
                          Id UNIQUEIDENTIFIER PRIMARY KEY,
                          Title NVARCHAR(200) NOT NULL,
                          Content NVARCHAR(MAX) NOT NULL,
                          Continent NVARCHAR(50) NOT NULL,
                          IsGlobal BIT NOT NULL
);
GO

USE AfricaDB;
CREATE TABLE Articles (
                          Id UNIQUEIDENTIFIER PRIMARY KEY,
                          Title NVARCHAR(200) NOT NULL,
                          Content NVARCHAR(MAX) NOT NULL,
                          Continent NVARCHAR(50) NOT NULL,
                          IsGlobal BIT NOT NULL
);
GO

USE NorthAmericaDB;
CREATE TABLE Articles (
                          Id UNIQUEIDENTIFIER PRIMARY KEY,
                          Title NVARCHAR(200) NOT NULL,
                          Content NVARCHAR(MAX) NOT NULL,
                          Continent NVARCHAR(50) NOT NULL,
                          IsGlobal BIT NOT NULL
);
GO