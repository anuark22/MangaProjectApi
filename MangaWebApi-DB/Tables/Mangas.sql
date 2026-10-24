CREATE TABLE [dbo].[Mangas]
(
	[MangaId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MangaTittle] VARCHAR(255) NOT NULL, 
    [MangaDescription] VARCHAR(MAX) NOT NULL, 
    [AlternativeNames] VARCHAR(MAX) NOT NULL, 
    [Mangaka] VARCHAR(MAX) NOT NULL, 
    [CreatedAt] DATETIME NOT NULL, 
    [UpdatedAt] DATETIME NULL, 
    [MangaFrontImgUrl] VARCHAR(255) NOT NULL, 
    [RegisteredBy] INT NOT NULL, 
    [MangaTypeId] INT NOT NULL, 
    [EditorialName] VARCHAR(155) NOT NULL, 
    [Rating] DECIMAL(3, 1) NOT NULL DEFAULT 0, 
    [Hidden] BIT NOT NULL DEFAULT 0, 
    CONSTRAINT [FK_Mangas_Users] FOREIGN KEY ([RegisteredBy]) REFERENCES [Users]([UserId]), 
    CONSTRAINT [FK_Mangas_MangaType] FOREIGN KEY ([MangaTypeId]) REFERENCES [Mangastype]([MangaTypeId])
)
