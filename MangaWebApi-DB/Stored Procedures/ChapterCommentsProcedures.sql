SET QUOTED_IDENTIFIER ON;
GO
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[ChapterCommentsProcedures]
    @CommentId INT = NULL,
    @ChapterId INT = NULL,
    @UserId INT = NULL,
    @ParentCommentId INT = NULL,
    @Content VARCHAR(2000) = NULL,
    @MediaUrl VARCHAR(500) = NULL,
    @IsAdmin BIT = 0,
    @Action INT
AS
BEGIN
    SET NOCOUNT ON;

    -- =========================================================================
    -- ACTION 1: SELECT Comments by ChapterId (Ordenados para armar árbol Reddit)
    -- =========================================================================
    IF @Action = 1
    BEGIN
        SELECT
            cc.[CommentId],
            cc.[ChapterId],
            cc.[UserId],
            u.[Username],
            u.[Email],
            u.[RoleId],
            cc.[ParentCommentId],
            CASE WHEN cc.[IsDeleted] = 1 THEN '[Comentario eliminado]' ELSE cc.[Content] END AS [Content],
            CASE WHEN cc.[IsDeleted] = 1 THEN NULL ELSE cc.[MediaUrl] END AS [MediaUrl],
            cc.[IsDeleted],
            cc.[CreatedAt]
        FROM [dbo].[ChapterComments] cc
        INNER JOIN [dbo].[Users] u ON cc.[UserId] = u.[UserId]
        WHERE cc.[ChapterId] = @ChapterId
        ORDER BY cc.[CreatedAt] ASC;
        RETURN;
    END

    -- =========================================================================
    -- ACTION 2: INSERT Comment / Reply
    -- =========================================================================
    IF @Action = 2
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[MangaChapters] WHERE [ChapterId] = @ChapterId)
        BEGIN
            SELECT 404 AS [Status], 'El capítulo no existe.' AS [ErrorDescription];
            RETURN;
        END

        IF @ParentCommentId IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM [dbo].[ChapterComments] WHERE [CommentId] = @ParentCommentId AND [ChapterId] = @ChapterId)
        BEGIN
            SELECT 404 AS [Status], 'El comentario padre al que intentas responder no existe.' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            INSERT INTO [dbo].[ChapterComments]
            (
                [ChapterId],
                [UserId],
                [ParentCommentId],
                [Content],
                [MediaUrl],
                [IsDeleted],
                [CreatedAt]
            )
            VALUES
            (
                @ChapterId,
                @UserId,
                @ParentCommentId,
                NULLIF(TRIM(@Content), ''),
                NULLIF(TRIM(@MediaUrl), ''),
                0,
                CURRENT_TIMESTAMP
            );

            DECLARE @NewId INT = SCOPE_IDENTITY();
            SELECT
                200 AS [Status],
                @NewId AS [NewCommentId],
                cc.[CommentId],
                cc.[ChapterId],
                cc.[UserId],
                u.[Username],
                u.[Email],
                u.[RoleId],
                cc.[ParentCommentId],
                cc.[Content],
                cc.[MediaUrl],
                cc.[IsDeleted],
                cc.[CreatedAt]
            FROM [dbo].[ChapterComments] cc
            INNER JOIN [dbo].[Users] u ON cc.[UserId] = u.[UserId]
            WHERE cc.[CommentId] = @NewId;
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
        RETURN;
    END

    -- =========================================================================
    -- ACTION 3: DELETE Comment (Soft delete si tiene respuestas hijas, físico si es hoja)
    -- =========================================================================
    IF @Action = 3
    BEGIN
        DECLARE @OwnerId INT;
        SELECT @OwnerId = [UserId] FROM [dbo].[ChapterComments] WHERE [CommentId] = @CommentId;

        IF @OwnerId IS NULL
        BEGIN
            SELECT 404 AS [Status], 'Comentario no encontrado.' AS [ErrorDescription];
            RETURN;
        END

        IF @OwnerId <> @UserId AND ISNULL(@IsAdmin, 0) = 0
        BEGIN
            SELECT 403 AS [Status], 'No tienes permisos para eliminar este comentario.' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            IF EXISTS (SELECT 1 FROM [dbo].[ChapterComments] WHERE [ParentCommentId] = @CommentId)
            BEGIN
                -- Preservar el árbol de respuestas hijas
                UPDATE [dbo].[ChapterComments]
                SET [IsDeleted] = 1, [Content] = NULL, [MediaUrl] = NULL
                WHERE [CommentId] = @CommentId;
            END
            ELSE
            BEGIN
                DELETE FROM [dbo].[ChapterComments] WHERE [CommentId] = @CommentId;
            END

            SELECT 200 AS [Status];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
        RETURN;
    END
END
RETURN 0;
