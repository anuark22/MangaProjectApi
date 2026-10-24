CREATE TABLE [dbo].[MangasDemographics]
(
	[DemographyId] INT NOT NULL, 
    [MangaId] INT NOT NULL, 
    CONSTRAINT [PK_MangasDemographics] PRIMARY KEY ([DemographyId], [MangaId]),
    CONSTRAINT [FK_MangasDemographics_Demographics] FOREIGN KEY ([DemographyId]) REFERENCES [Demographics]([DemographyId]), 
    CONSTRAINT [FK_MangasDemographics_Mangas] FOREIGN KEY ([MangaId]) REFERENCES [Mangas]([MangaId])
)
