CREATE DATABASE CommentGlobalDB;
CREATE DATABASE CommentAntarcticaDB;
CREATE DATABASE CommentAustraliaDB;
CREATE DATABASE CommentSouthAmericaDB;
CREATE DATABASE CommentEuropeDB;
CREATE DATABASE CommentAsiaDB;
CREATE DATABASE CommentAfricaDB;
CREATE DATABASE CommentNorthAmericaDB;

GO

USE CommentGlobalDB;
CREATE TABLE Comments (
                          Id UNIQUEIDENTIFIER PRIMARY KEY,
                          Content NVARCHAR(MAX) NOT NULL
);
GO

USE CommentAntarticaDB;
CREATE TABLE Comments (
                          Id UNIQUEIDENTIFIER PRIMARY KEY,
                          Content NVARCHAR(MAX) NOT NULL
);
GO

USE CommentAustraliaDB;
CREATE TABLE Comments (
                          Id UNIQUEIDENTIFIER PRIMARY KEY,
                          Content NVARCHAR(MAX) NOT NULL
);
GO

USE CommentSouthAmericaDB;
CREATE TABLE Comments (
                          Id UNIQUEIDENTIFIER PRIMARY KEY,
                          Content NVARCHAR(MAX) NOT NULL
);
GO

USE CommentEuropeDB;
CREATE TABLE Comments (
                          Id UNIQUEIDENTIFIER PRIMARY KEY,
                          Content NVARCHAR(MAX) NOT NULL
);
GO

USE CommentAsiaDB;
CREATE TABLE Comments (
                          Id UNIQUEIDENTIFIER PRIMARY KEY,
                          Content NVARCHAR(MAX) NOT NULL
);
GO

USE CommentAfricaDB;
CREATE TABLE Comments (
                          Id UNIQUEIDENTIFIER PRIMARY KEY,
                          Content NVARCHAR(MAX) NOT NULL
);
GO

USE CommentNorthAmericaDB;
CREATE TABLE Comments (
                          Id UNIQUEIDENTIFIER PRIMARY KEY,
                          Content NVARCHAR(MAX) NOT NULL
);
GO