CREATE DATABASE CommentGlobalDB;

GO

USE CommentGlobalDB;
CREATE TABLE Comments (
                          Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                          ArticleId UNIQUEIDENTIFIER NOT NULL,
                          Content NVARCHAR(MAX) NOT NULL,
                          CreatedAt DATETIME2 DEFAULT GETDATE() 
);
GO