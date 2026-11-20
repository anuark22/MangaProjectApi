IF OBJECT_ID('dbo.AuditLogs', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AuditLogs] (
        [AuditId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId] INT NULL,
        [Username] NVARCHAR(150) NULL,
        [UserRole] NVARCHAR(50) NULL,
        [ScanId] INT NULL,
        [Action] NVARCHAR(50) NOT NULL,
        [EntityType] NVARCHAR(80) NOT NULL,
        [EntityId] INT NULL,
        [EntityName] NVARCHAR(250) NULL,
        [Details] NVARCHAR(MAX) NULL,
        [IpAddress] NVARCHAR(64) NULL,
        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_AuditLogs_CreatedAt] DEFAULT (GETUTCDATE())
    );

    CREATE NONCLUSTERED INDEX [IX_AuditLogs_CreatedAt]
        ON [dbo].[AuditLogs] ([CreatedAt] DESC);

    CREATE NONCLUSTERED INDEX [IX_AuditLogs_Entity_Action]
        ON [dbo].[AuditLogs] ([EntityType], [Action]);
END
GO
