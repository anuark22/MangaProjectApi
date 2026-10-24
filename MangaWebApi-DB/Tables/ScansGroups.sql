CREATE TABLE [dbo].[ScansGroups]
(
    [ScanId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [ScanName] VARCHAR(255) NOT NULL, 
    [ScanDescription] VARCHAR(MAX) NOT NULL, 
    [CreatedBy] INT NOT NULL, 
    [EmailNotificaction] BIT NOT NULL DEFAULT 0, 
    [ScanImgUrl] VARCHAR(255) NOT NULL, 
    [ScanStatus] BIT NOT NULL DEFAULT 0
)
