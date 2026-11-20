CREATE TABLE [dbo].[UserReadChapters]
(
    [ReadId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [UserId] INT NOT NULL,
    [MangaId] INT NOT NULL,
    [ChapterId] INT NOT NULL,
    [ReadAt] DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT [FK_UserReadChapters_Users] FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId]),
    CONSTRAINT [FK_UserReadChapters_Mangas] FOREIGN KEY ([MangaId]) REFERENCES [Mangas]([MangaId]),
    CONSTRAINT [FK_UserReadChapters_Chapters] FOREIGN KEY ([ChapterId]) REFERENCES [MangaChapters]([ChapterId]) ON DELETE CASCADE,
    CONSTRAINT [UQ_UserReadChapters_User_Chapter] UNIQUE ([UserId], [ChapterId])
);
GO

CREATE INDEX [IX_UserReadChapters_User_Manga]
ON [dbo].[UserReadChapters]([UserId], [MangaId]);
GO
