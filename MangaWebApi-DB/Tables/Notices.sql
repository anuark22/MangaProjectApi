CREATE TABLE [dbo].[Notices] (
    [NoticeId] INT IDENTITY(1,1) PRIMARY KEY,
    [Title] VARCHAR(255) NOT NULL,
    [Content] VARCHAR(MAX) NOT NULL,
    [NoticeImgUrl] VARCHAR(255) NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    [CreatedBy] INT NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [FK_Notices_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [Users]([UserId])
);
