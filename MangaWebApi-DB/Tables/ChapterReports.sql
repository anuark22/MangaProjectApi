CREATE TABLE [dbo].[ChapterReports]
(
    [ReportId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [ChapterId] INT NULL,
    [ChapterNumber] INT NOT NULL,
    [ChapterTitle] VARCHAR(255) NULL,
    [MangaId] INT NOT NULL,
    [ScanId] INT NOT NULL,
    [UserId] INT NOT NULL,
    [Reason] VARCHAR(100) NOT NULL,
    [Description] VARCHAR(1000) NULL,
    [Status] VARCHAR(20) NOT NULL DEFAULT 'PENDING',
    [AdminNotes] VARCHAR(1000) NULL,
    [ActionTaken] VARCHAR(100) NULL,
    [ReviewedBy] INT NULL,
    [ReviewedAt] DATETIME NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    [UpdatedAt] DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT [FK_ChapterReports_MangaChapters] FOREIGN KEY ([ChapterId]) REFERENCES [MangaChapters]([ChapterId]) ON DELETE SET NULL,
    CONSTRAINT [FK_ChapterReports_Mangas] FOREIGN KEY ([MangaId]) REFERENCES [Mangas]([MangaId]),
    CONSTRAINT [FK_ChapterReports_Scans] FOREIGN KEY ([ScanId]) REFERENCES [ScansGroups]([ScanId]),
    CONSTRAINT [FK_ChapterReports_Users] FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId]),
    CONSTRAINT [FK_ChapterReports_ReviewedBy] FOREIGN KEY ([ReviewedBy]) REFERENCES [Users]([UserId])
);
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE UNIQUE INDEX [UX_ChapterReports_Chapter_User]
ON [dbo].[ChapterReports]([ChapterId], [UserId])
WHERE [ChapterId] IS NOT NULL;
GO
