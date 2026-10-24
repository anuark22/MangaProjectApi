CREATE TABLE [dbo].[MangaChapters]
(
	[ChapterId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [ChapterNumber] INT NOT NULL, 
    [ChapterTitle] VARCHAR(255) NULL, 
    [MangaId] INT NOT NULL, 
    [UploadedBy] INT NOT NULL, 
    [UploadedAt] DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, 
    [ScanId] INT NOT NULL, 
    [ViewAmount] INT NOT NULL, 
    [Rating] DECIMAL(3, 1) NOT NULL DEFAULT 0, 
    [ChapterStatus] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [FK_MangaChapters_Users] FOREIGN KEY ([UploadedBy]) REFERENCES [Users]([UserId]), 
    CONSTRAINT [FK_MangaChapters_Mangas] FOREIGN KEY ([MangaId]) REFERENCES [Mangas]([MangaId])
)
