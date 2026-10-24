CREATE PROCEDURE [dbo].[MangaChaptersProcedures]
    @ChapterId INT = NULL,
    @ChapterNumber INT = NULL,
    @ChapterTitle VARCHAR(255) = NULL,
    @MangaId INT = NULL,
    @UploadedBy INT = NULL,
    @ScanId INT = NULL,
    @ViewAmount INT = NULL,
    @ChapterStatus BIT = NULL,
    @Action INT 
AS
BEGIN
    SET NOCOUNT ON;

    -- =========================================================================
    -- ACTION 1: SELECT (Buscar capítulos por Manga o ID)
    -- =========================================================================
    IF @Action = 1
    BEGIN
        SELECT 
            [ChapterId],
            [ChapterNumber],
            [ChapterTitle],
            [MangaId],
            [UploadedBy],
            [UploadedAt],
            [ScanId],
            [ViewAmount],
            [Rating],
            [ChapterStatus]
        FROM [dbo].[MangaChapters]
        WHERE 
            (@ChapterId IS NULL OR [ChapterId] = @ChapterId)
            AND (@MangaId IS NULL OR [MangaId] = @MangaId)
            AND (@ChapterNumber IS NULL OR [ChapterNumber] = @ChapterNumber)
    END

    -- =========================================================================
    -- ACTION 2: INSERT (Crear capítulo con validación de Scan e integridad)
    -- =========================================================================
    IF @Action = 2
    BEGIN
        -- Validación: Un Scan solo puede subir 1 capítulo con el mismo número para el mismo Manga
        IF EXISTS (SELECT 1 FROM [dbo].[MangaChapters] WHERE [MangaId] = @MangaId AND [ChapterNumber] = @ChapterNumber AND [ScanId] = @ScanId)
        BEGIN
            SELECT 409 AS [Status], 'This scan has already uploaded this chapter number for this manga' AS [ErrorDescription];
            RETURN;
        END

        -- Validación: Verificar que el Manga exista
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Mangas] WHERE [MangaId] = @MangaId)
        BEGIN
            SELECT 404 AS [Status], 'Manga not found' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            INSERT INTO [dbo].[MangaChapters] 
            (
                [ChapterNumber], 
                [ChapterTitle], 
                [MangaId], 
                [UploadedBy], 
                [UploadedAt], 
                [ScanId], 
                [ViewAmount], 
                [Rating], 
                [ChapterStatus]
            )
            VALUES 
            (
                @ChapterNumber, 
                TRIM(@ChapterTitle), 
                @MangaId, 
                @UploadedBy, 
                CURRENT_TIMESTAMP, 
                @ScanId, 
                0, -- ViewAmount inicial
                0.0, -- Rating inicial
                ISNULL(@ChapterStatus, 1)
            );

            SELECT 200 AS [Status], SCOPE_IDENTITY() AS [NewChapterId];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END

    -- =========================================================================
    -- ACTION 3: UPDATE (Actualizar datos del capítulo)
    -- =========================================================================
    IF @Action = 3
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[MangaChapters] WHERE [ChapterId] = @ChapterId)
        BEGIN
            SELECT 404 AS [Status], 'Chapter not found' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            UPDATE [dbo].[MangaChapters]
            SET 
                [ChapterNumber] = ISNULL(@ChapterNumber, [ChapterNumber]),
                [ChapterTitle] = ISNULL(TRIM(@ChapterTitle), [ChapterTitle]),
                [ChapterStatus] = ISNULL(@ChapterStatus, [ChapterStatus])
            WHERE 
                [ChapterId] = @ChapterId;

            SELECT 200 AS [Status];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END
    
    -- =========================================================================
    -- ACTION 4: DELETE (Borrado físico del capítulo y sus páginas en cascada)
    -- =========================================================================
    IF @Action = 4
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[MangaChapters] WHERE [ChapterId] = @ChapterId)
        BEGIN
            SELECT 404 AS [Status], 'Chapter not found' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            -- Borramos primero las páginas asociadas
            DELETE FROM [dbo].[MangaPages] WHERE [ChapterId] = @ChapterId;
            -- Borramos el capítulo
            DELETE FROM [dbo].[MangaChapters] WHERE [ChapterId] = @ChapterId;

            SELECT 200 AS [Status];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END

END
RETURN 0;