CREATE TABLE [dbo].[Users]
(
	[UserId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Username] VARCHAR(255) NOT NULL, 
    [Email] VARCHAR(255) NOT NULL UNIQUE, 
    [Password] VARCHAR(255) NOT NULL, 
    [RoleId] INT NOT NULL, 
    [Status] BIT NOT NULL DEFAULT 1, 
    [CreatedAt] DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, 
    [UpdatedAt] DATETIME NULL, 
    [RecoveryToken] VARCHAR(MAX) NULL, 
    [AceptTerms] BIT NOT NULL, 
    [IsBlocked] BIT NOT NULL DEFAULT 0, 
    [ScanId] INT NULL, 
    CONSTRAINT [FK_Users_Roles] FOREIGN KEY ([RoleId]) REFERENCES [Roles]([RoleId])
)
