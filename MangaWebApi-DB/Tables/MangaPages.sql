CREATE TABLE [dbo].[MangaPages]
(
    [PageId] INT NOT NULL PRIMARY KEY IDENTITY,
    [ChapterId] INT NOT NULL,
    [PageNumber] INT NOT NULL,
    [PageImgUrl] VARCHAR(MAX) NOT NULL,
    CONSTRAINT [FK_MangaPages_MangaChapters] FOREIGN KEY ([ChapterId]) REFERENCES [MangaChapters]([ChapterId]) ON DELETE CASCADE
)