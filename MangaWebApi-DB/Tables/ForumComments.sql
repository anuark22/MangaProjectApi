CREATE TABLE [dbo].[ForumComments] (
    [CommentId] INT IDENTITY(1,1) PRIMARY KEY,
    [PostId] INT NOT NULL,
    [UserId] INT NOT NULL,
    [Content] VARCHAR(MAX) NOT NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT [FK_ForumComments_ForumPosts] FOREIGN KEY ([PostId]) REFERENCES [ForumPosts]([PostId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ForumComments_Users] FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId])
);
