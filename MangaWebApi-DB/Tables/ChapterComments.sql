CREATE TABLE [dbo].[ChapterComments]
(
    [CommentId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [ChapterId] INT NOT NULL,
    [UserId] INT NOT NULL,
    [ParentCommentId] INT NULL,
    [Content] VARCHAR(2000) NULL,
    [MediaUrl] VARCHAR(500) NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT [FK_ChapterComments_Chapters] FOREIGN KEY ([ChapterId]) REFERENCES [MangaChapters]([ChapterId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ChapterComments_Users] FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId]),
    CONSTRAINT [FK_ChapterComments_Parent] FOREIGN KEY ([ParentCommentId]) REFERENCES [ChapterComments]([CommentId])
);
GO

CREATE INDEX [IX_ChapterComments_ChapterId_CreatedAt]
ON [dbo].[ChapterComments]([ChapterId], [CreatedAt] ASC);
GO
