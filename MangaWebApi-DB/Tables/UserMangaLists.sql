CREATE TABLE [dbo].[UserMangaLists] (
    [UserListId] INT IDENTITY(1,1) PRIMARY KEY,
    [UserId] INT NOT NULL,
    [MangaId] INT NOT NULL,
    [ReadingStatus] VARCHAR(50) NOT NULL DEFAULT 'PlanToRead',
    [CreatedAt] DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT [FK_UserMangaLists_Users] FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId]),
    CONSTRAINT [FK_UserMangaLists_Mangas] FOREIGN KEY ([MangaId]) REFERENCES [Mangas]([MangaId]),
    CONSTRAINT [UQ_User_Manga] UNIQUE([UserId], [MangaId])
);
