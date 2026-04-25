CREATE DATABASE SubscriberDatabase;
GO

USE SubscriberDatabase;
CREATE TABLE Subscriber(
                       Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                       Email NVARCHAR(200) NOT NULL,
                       Continent NVARCHAR(50) NOT NULL,
                       SubscribedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO