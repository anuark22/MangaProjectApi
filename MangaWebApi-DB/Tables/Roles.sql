CREATE TABLE [dbo].[Roles]
(
	[RoleId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [RoleName] VARCHAR(45) NOT NULL, 
    [RoleDescription] VARCHAR(100) NOT NULL, 
    [IsActive] TINYINT NOT NULL, 
    [IsOwnerRole] TINYINT NOT NULL
)
