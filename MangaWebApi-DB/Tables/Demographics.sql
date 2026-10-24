CREATE TABLE [dbo].[Demographics]
(
	[DemographyId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] VARCHAR(255) NOT NULL, 
    [Description] VARCHAR(MAX) NOT NULL, 
    [DemographyImgUrl] VARCHAR(MAX) NOT NULL
)
