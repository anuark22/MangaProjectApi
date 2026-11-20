CREATE TABLE [dbo].[LiveChatMessages] (
    [MessageId] NVARCHAR(32) NOT NULL PRIMARY KEY,
    [UserId] INT NOT NULL,
    [Username] NVARCHAR(150) NOT NULL,
    [Email] NVARCHAR(255) NOT NULL CONSTRAINT [DF_LiveChatMessages_Email] DEFAULT (''),
    [RoleId] INT NOT NULL CONSTRAINT [DF_LiveChatMessages_RoleId] DEFAULT (1),
    [Text] NVARCHAR(500) NOT NULL CONSTRAINT [DF_LiveChatMessages_Text] DEFAULT (''),
    [MediaUrl] NVARCHAR(1000) NULL,
    [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_LiveChatMessages_CreatedAt] DEFAULT (SYSUTCDATETIME())
);
GO

CREATE NONCLUSTERED INDEX [IX_LiveChatMessages_CreatedAt]
    ON [dbo].[LiveChatMessages] ([CreatedAt] DESC);
GO
