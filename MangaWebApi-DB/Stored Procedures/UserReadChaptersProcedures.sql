SET QUOTED_IDENTIFIER ON;
GO
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[UserReadChaptersProcedures]
    @UserId INT = NULL,
    @MangaId INT = NULL,
    @ChapterId INT = NULL,
    @Action INT
AS
BEGIN
    SET NOCOUNT ON;

    -- =========================================================================
    -- ACTION 1: SELECT Read Chapters for a User & Manga
    -- =========================================================================
    IF @Action = 1
    BEGIN
        SELECT
            [ReadId],
            [UserId],
            [MangaId],
            [ChapterId],
            [ReadAt]
        FROM [dbo].[UserReadChapters]
        WHERE [UserId] = @UserId
          AND (@MangaId IS NULL OR [MangaId] = @MangaId)
        ORDER BY [ReadAt] DESC;
        RETURN;
    END

    -- =========================================================================
    -- ACTION 2: MARK Chapter as Read (SOLO si el usuario sigue el manga en UserMangaLists)
    -- =========================================================================
    IF @Action = 2
    BEGIN
        -- Regla de negocio: Solo registrar progreso si el usuario está siguiendo este manga
        IF NOT EXISTS (SELECT 1 FROM [dbo].[UserMangaLists] WHERE [UserId] = @UserId AND [MangaId] = @MangaId)
        BEGIN
            SELECT 200 AS [Status], CAST(0 AS BIT) AS [Marked];
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM [dbo].[MangaChapters] WHERE [ChapterId] = @ChapterId AND [MangaId] = @MangaId)
        BEGIN
            SELECT 404 AS [Status], 'Capítulo no encontrado para este manga.' AS [ErrorDescription];
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM [dbo].[UserReadChapters] WHERE [UserId] = @UserId AND [ChapterId] = @ChapterId)
        BEGIN
            INSERT INTO [dbo].[UserReadChapters] ([UserId], [MangaId], [ChapterId], [ReadAt])
            VALUES (@UserId, @MangaId, @ChapterId, CURRENT_TIMESTAMP);
        END

        SELECT 200 AS [Status], CAST(1 AS BIT) AS [Marked];
        RETURN;
    END

    -- =========================================================================
    -- ACTION 3: UNMARK Chapter as Read
    -- =========================================================================
    IF @Action = 3
    BEGIN
        DELETE FROM [dbo].[UserReadChapters]
        WHERE [UserId] = @UserId
          AND [ChapterId] = @ChapterId;

        SELECT 200 AS [Status];
        RETURN;
    END

    -- =========================================================================
    -- ACTION 4: GET Reading Progress summary across all followed mangas
    -- =========================================================================
    IF @Action = 4
    BEGIN
        SELECT
            uml.[MangaId],
            uml.[ReadingStatus],
            (SELECT COUNT(DISTINCT mc.[ChapterNumber])
             FROM [dbo].[MangaChapters] mc
             WHERE mc.[MangaId] = uml.[MangaId] AND mc.[ChapterStatus] = 1) AS [TotalChapters],
            (SELECT COUNT(DISTINCT mc2.[ChapterNumber])
             FROM [dbo].[UserReadChapters] urc
             INNER JOIN [dbo].[MangaChapters] mc2 ON urc.[ChapterId] = mc2.[ChapterId]
             WHERE urc.[UserId] = @UserId AND urc.[MangaId] = uml.[MangaId]) AS [ReadChaptersCount],
            (SELECT MAX(mc3.[ChapterNumber])
             FROM [dbo].[UserReadChapters] urc2
             INNER JOIN [dbo].[MangaChapters] mc3 ON urc2.[ChapterId] = mc3.[ChapterId]
             WHERE urc2.[UserId] = @UserId AND urc2.[MangaId] = uml.[MangaId]) AS [LastReadChapterNumber]
        FROM [dbo].[UserMangaLists] uml
        WHERE uml.[UserId] = @UserId;
        RETURN;
    END
END
RETURN 0;
