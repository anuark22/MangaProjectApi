CREATE OR ALTER PROCEDURE [dbo].[ForumProcedures]
    @PostId INT = NULL,
    @CommentId INT = NULL,
    @UserId INT = NULL,
    @Title VARCHAR(255) = NULL,
    @Content VARCHAR(MAX) = NULL,
    @Topic VARCHAR(100) = NULL,
    @SubTopic VARCHAR(100) = NULL,
    @SortBy VARCHAR(50) = 'Latest',
    @Action INT
AS
BEGIN
    SET NOCOUNT ON;

    -- =========================================================================
    -- ACTION 1: SELECT (Listar posts con filtros de Topic, SubTopic y Orden)
    -- =========================================================================
    IF @Action = 1
    BEGIN
        SELECT 
            fp.[PostId],
            fp.[Title],
            fp.[Content],
            fp.[Topic],
            fp.[SubTopic],
            fp.[UserId],
            ISNULL(u.[Username], 'Unknown') AS [UserName],
            ISNULL(u.[Email], '') AS [Email],
            fp.[ViewsCount],
            (SELECT COUNT(1) FROM [dbo].[ForumComments] fc WHERE fc.[PostId] = fp.[PostId]) AS [PostCommentsCount],
            fp.[CreatedAt],
            fp.[UpdatedAt],
            fp.[IsPinned],
            fp.[IsClosed]
        FROM [dbo].[ForumPosts] fp
        LEFT JOIN [dbo].[Users] u ON fp.[UserId] = u.[UserId]
        WHERE 
            (@PostId IS NULL OR fp.[PostId] = @PostId)
            AND (@Topic IS NULL OR fp.[Topic] = @Topic)
            AND (@SubTopic IS NULL OR fp.[SubTopic] = @SubTopic)
        ORDER BY 
            fp.[IsPinned] DESC,
            CASE WHEN @SortBy = 'Older' THEN fp.[CreatedAt] END ASC,
            CASE WHEN @SortBy = 'Popular' THEN fp.[ViewsCount] END DESC,
            CASE WHEN @SortBy = 'Discussions' THEN (SELECT COUNT(1) FROM [dbo].[ForumComments] fc WHERE fc.[PostId] = fp.[PostId]) END DESC,
            fp.[CreatedAt] DESC;
    END

    -- =========================================================================
    -- ACTION 2: INSERT (Crear nueva discusión)
    -- =========================================================================
    IF @Action = 2
    BEGIN
        BEGIN TRY
            INSERT INTO [dbo].[ForumPosts] 
            (
                [Title], 
                [Content], 
                [Topic], 
                [SubTopic], 
                [UserId], 
                [ViewsCount], 
                [CreatedAt], 
                [IsPinned], 
                [IsClosed]
            )
            VALUES 
            (
                TRIM(@Title), 
                TRIM(@Content), 
                TRIM(@Topic), 
                TRIM(@SubTopic), 
                @UserId, 
                0, 
                CURRENT_TIMESTAMP, 
                0, 
                0
            );

            SELECT 200 AS [Status], SCOPE_IDENTITY() AS [NewPostId];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END

    -- =========================================================================
    -- ACTION 3: UPDATE (Editar discusión)
    -- =========================================================================
    IF @Action = 3
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[ForumPosts] WHERE [PostId] = @PostId)
        BEGIN
            SELECT 404 AS [Status], 'Discussion not found' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            UPDATE [dbo].[ForumPosts]
            SET 
                [Title] = ISNULL(TRIM(@Title), [Title]),
                [Content] = ISNULL(TRIM(@Content), [Content]),
                [Topic] = ISNULL(TRIM(@Topic), [Topic]),
                [SubTopic] = ISNULL(TRIM(@SubTopic), [SubTopic]),
                [UpdatedAt] = CURRENT_TIMESTAMP
            WHERE [PostId] = @PostId AND [UserId] = @UserId;

            SELECT 200 AS [Status];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END

    -- =========================================================================
    -- ACTION 4: DELETE (Eliminar discusión)
    -- =========================================================================
    IF @Action = 4
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[ForumPosts] WHERE [PostId] = @PostId)
        BEGIN
            SELECT 404 AS [Status], 'Discussion not found' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            DELETE FROM [dbo].[ForumPosts] WHERE [PostId] = @PostId;
            SELECT 200 AS [Status];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END

    -- =========================================================================
    -- ACTION 5: INCREMENT VIEW (Incrementar contador de visitas)
    -- =========================================================================
    IF @Action = 5
    BEGIN
        UPDATE [dbo].[ForumPosts] 
        SET [ViewsCount] = [ViewsCount] + 1 
        WHERE [PostId] = @PostId;

        SELECT 200 AS [Status];
    END

    -- =========================================================================
    -- ACTION 6: GET COMMENTS (Obtener comentarios de una discusión)
    -- =========================================================================
    IF @Action = 6
    BEGIN
        SELECT 
            fc.[CommentId],
            fc.[PostId],
            fc.[UserId],
            fc.[Content],
            fc.[CreatedAt],
            ISNULL(u.[Username], 'Unknown') AS [UserName],
            ISNULL(u.[Email], '') AS [Email]
        FROM [dbo].[ForumComments] fc
        LEFT JOIN [dbo].[Users] u ON fc.[UserId] = u.[UserId]
        WHERE fc.[PostId] = @PostId
        ORDER BY fc.[CreatedAt] ASC;
    END

    -- =========================================================================
    -- ACTION 7: ADD COMMENT (Agregar comentario a una discusión)
    -- =========================================================================
    IF @Action = 7
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[ForumPosts] WHERE [PostId] = @PostId)
        BEGIN
            SELECT 404 AS [Status], 'Discussion not found' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            INSERT INTO [dbo].[ForumComments] ([PostId], [UserId], [Content], [CreatedAt])
            VALUES (@PostId, @UserId, TRIM(@Content), CURRENT_TIMESTAMP);

            SELECT 200 AS [Status], SCOPE_IDENTITY() AS [NewCommentId];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END

    -- =========================================================================
    -- ACTION 8: DELETE COMMENT (Eliminar comentario)
    -- =========================================================================
    IF @Action = 8
    BEGIN
        BEGIN TRY
            DELETE FROM [dbo].[ForumComments] 
            WHERE [CommentId] = @CommentId;

            SELECT 200 AS [Status];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END
END
RETURN 0;
