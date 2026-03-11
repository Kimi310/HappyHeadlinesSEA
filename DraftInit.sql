CREATE DATABASE DraftsDatabase;
GO

USE DraftsDatabase;
CREATE TABLE Drafts(
                       Id UNIQUEIDENTIFIER PRIMARY KEY,
                       Title NVARCHAR(200) NOT NULL,
                       Content NVARCHAR(MAX) NOT NULL,
                       Continent NVARCHAR(50) NOT NULL,
                       IsGlobal BIT NOT NULL
);
GO