CREATE DATABASE CommentGlobalDB;

GO

USE CommentGlobalDB;
CREATE TABLE Comments (
                          Id UNIQUEIDENTIFIER PRIMARY KEY,
                          Content NVARCHAR(MAX) NOT NULL
);
GO