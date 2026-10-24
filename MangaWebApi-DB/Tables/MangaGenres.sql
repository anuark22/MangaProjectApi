CREATE TABLE [dbo].[MangaGenres]
(
	[GenreId] INT NOT NULL, 
    [MangaId] INT NOT NULL, 
    CONSTRAINT [PK_MangaGenres] PRIMARY KEY ([GenreId],[MangaId]),
    CONSTRAINT [FK_MangaGenres_Mangas] FOREIGN KEY ([MangaId]) REFERENCES [Mangas]([MangaId]), 
    CONSTRAINT [FK_MangaGenres_Genres] FOREIGN KEY ([GenreId]) REFERENCES [Genres]([GenreId]),
)
