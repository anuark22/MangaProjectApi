CREATE TABLE [dbo].[ScanGroupApplications]
(
    [ApplicationId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [UserId] INT NOT NULL,
    [ScanName] VARCHAR(255) NOT NULL,
    [ScanDescription] VARCHAR(MAX) NOT NULL,
    [ScanImgUrl] VARCHAR(255) NOT NULL,
    [EmailNotification] BIT NOT NULL DEFAULT 0,
    [DiscordUrl] VARCHAR(255) NULL,
    [WebsiteUrl] VARCHAR(255) NULL,
    [ApplicationMessage] VARCHAR(MAX) NULL,
    [Status] VARCHAR(20) NOT NULL DEFAULT 'PENDING',
    [AdminNotes] VARCHAR(MAX) NULL,
    [ReviewedBy] INT NULL,
    [ReviewedAt] DATETIME NULL,
    [CreatedScanId] INT NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    [UpdatedAt] DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT [FK_ScanGroupApplications_Users] FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId]),
    CONSTRAINT [FK_ScanGroupApplications_ReviewedBy] FOREIGN KEY ([ReviewedBy]) REFERENCES [Users]([UserId]),
    CONSTRAINT [FK_ScanGroupApplications_Scans] FOREIGN KEY ([CreatedScanId]) REFERENCES [ScansGroups]([ScanId])
);
