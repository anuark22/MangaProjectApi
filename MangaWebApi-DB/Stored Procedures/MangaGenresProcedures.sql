CREATE PROCEDURE [dbo].[MangaGenresProcedures]
    @GenreId INT = NULL,
    @MangaId INT = NULL,
    @Action INT 
AS
BEGIN
    SET NOCOUNT ON;

    -- =========================================================================
    -- ACTION 1: SELECT (Obtener relaciones, filtrando por género o manga)
    -- =========================================================================
    IF @Action = 1
    BEGIN
        SELECT 
            [GenreId],
            [MangaId]
        FROM [dbo].[MangaGenres]
        WHERE 
            (@GenreId IS NULL OR [GenreId] = @GenreId)
            AND (@MangaId IS NULL OR [MangaId] = @MangaId)
    END

    -- =========================================================================
    -- ACTION 2: INSERT (Asociar un género a un manga)
    -- =========================================================================
    IF @Action = 2
    BEGIN
        -- Validación: La relación ya existe
        IF EXISTS (SELECT 1 FROM [dbo].[MangaGenres] WHERE [GenreId] = @GenreId AND [MangaId] = @MangaId)
        BEGIN
            SELECT 409 AS [Status], 'This manga is already associated with this genre' AS [ErrorDescription];
            RETURN;
        END

        -- Validación: Verificar que el género exista
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Genres] WHERE [GenreId] = @GenreId)
        BEGIN
            SELECT 404 AS [Status], 'Genre not found' AS [ErrorDescription];
            RETURN;
        END

        -- Validación: Verificar que el manga exista
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Mangas] WHERE [MangaId] = @MangaId)
        BEGIN
            SELECT 404 AS [Status], 'Manga not found' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            INSERT INTO [dbo].[MangaGenres] ([GenreId], [MangaId])
            VALUES (@GenreId, @MangaId);

            SELECT 200 AS [Status];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END

    -- =========================================================================
    -- ACTION 3: DELETE (Eliminar la asociación entre género y manga)
    -- =========================================================================
    IF @Action = 3
    BEGIN
        -- Validación: Verificar que la relación exista
        IF NOT EXISTS (SELECT 1 FROM [dbo].[MangaGenres] WHERE [GenreId] = @GenreId AND [MangaId] = @MangaId)
        BEGIN
            SELECT 404 AS [Status], 'Relationship not found' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            DELETE FROM [dbo].[MangaGenres]
            WHERE [GenreId] = @GenreId AND [MangaId] = @MangaId;

            SELECT 200 AS [Status];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END

END
RETURN 0;