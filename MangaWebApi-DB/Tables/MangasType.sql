CREATE TABLE [dbo].[MangasType]
(
	[MangaTypeId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MangaTypeName] VARCHAR(255) NOT NULL, 
    [MangaTypeImgUrl] VARCHAR(255) NOT NULL, 
    [Status] BIT NOT NULL DEFAULT 0
)
