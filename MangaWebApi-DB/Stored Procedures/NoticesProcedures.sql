CREATE OR ALTER PROCEDURE [dbo].[NoticesProcedures]
    @NoticeId INT = NULL,
    @Title VARCHAR(255) = NULL,
    @Content VARCHAR(MAX) = NULL,
    @NoticeImgUrl VARCHAR(255) = NULL,
    @CreatedBy INT = NULL,
    @IsActive BIT = NULL,
    @Action INT
AS
BEGIN
    SET NOCOUNT ON;

    -- =========================================================================
    -- ACTION 1: SELECT (Listar noticias activas o por Id)
    -- =========================================================================
    IF @Action = 1
    BEGIN
        SELECT 
            n.[NoticeId],
            n.[Title],
            n.[Content],
            n.[NoticeImgUrl],
            n.[CreatedAt],
            n.[CreatedBy],
            ISNULL(u.[Username], 'Admin') AS [AuthorName],
            n.[IsActive]
        FROM [dbo].[Notices] n
        LEFT JOIN [dbo].[Users] u ON n.[CreatedBy] = u.[UserId]
        WHERE 
            (@NoticeId IS NULL OR n.[NoticeId] = @NoticeId)
            AND (@IsActive IS NULL OR n.[IsActive] = @IsActive)
        ORDER BY n.[CreatedAt] DESC, n.[NoticeId] DESC;
    END

    -- =========================================================================
    -- ACTION 2: INSERT (Crear noticia)
    -- =========================================================================
    IF @Action = 2
    BEGIN
        BEGIN TRY
            INSERT INTO [dbo].[Notices] 
            (
                [Title], 
                [Content], 
                [NoticeImgUrl], 
                [CreatedAt], 
                [CreatedBy], 
                [IsActive]
            )
            VALUES 
            (
                TRIM(@Title), 
                TRIM(@Content), 
                TRIM(@NoticeImgUrl), 
                CURRENT_TIMESTAMP, 
                @CreatedBy, 
                ISNULL(@IsActive, 1)
            );

            SELECT 200 AS [Status], SCOPE_IDENTITY() AS [NewNoticeId];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END

    -- =========================================================================
    -- ACTION 3: UPDATE (Actualizar noticia)
    -- =========================================================================
    IF @Action = 3
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Notices] WHERE [NoticeId] = @NoticeId)
        BEGIN
            SELECT 404 AS [Status], 'Notice not found' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            UPDATE [dbo].[Notices]
            SET 
                [Title] = ISNULL(TRIM(@Title), [Title]),
                [Content] = ISNULL(TRIM(@Content), [Content]),
                [NoticeImgUrl] = ISNULL(TRIM(@NoticeImgUrl), [NoticeImgUrl]),
                [IsActive] = ISNULL(@IsActive, [IsActive])
            WHERE [NoticeId] = @NoticeId;

            SELECT 200 AS [Status];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END

    -- =========================================================================
    -- ACTION 4: DELETE (Eliminar noticia)
    -- =========================================================================
    IF @Action = 4
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Notices] WHERE [NoticeId] = @NoticeId)
        BEGIN
            SELECT 404 AS [Status], 'Notice not found' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            DELETE FROM [dbo].[Notices] WHERE [NoticeId] = @NoticeId;
            SELECT 200 AS [Status];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END
END
RETURN 0;
