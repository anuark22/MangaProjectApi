CREATE OR ALTER PROCEDURE [dbo].[UserMangaListsProcedures]
    @UserListId INT = NULL,
    @UserId INT = NULL,
    @MangaId INT = NULL,
    @ReadingStatus VARCHAR(50) = NULL,
    @Action INT
AS
BEGIN
    SET NOCOUNT ON;

    -- =========================================================================
    -- ACTION 1: SELECT (Obtener mangas en la lista del usuario)
    -- =========================================================================
    IF @Action = 1
    BEGIN
        SELECT 
            uml.[UserListId],
            uml.[UserId],
            uml.[MangaId],
            uml.[ReadingStatus],
            uml.[CreatedAt] AS [AddedAt],
            m.[MangaTittle],
            m.[MangaDescription],
            m.[MangaFrontImgUrl],
            m.[Mangaka],
            m.[Rating],
            m.[Status] AS [MangaStatus],
            ISNULL(mt.[MangaTypeName], '') AS [MangaTypeName],
            ISNULL(d.[Name], '') AS [DemographyName]
        FROM [dbo].[UserMangaLists] uml
        INNER JOIN [dbo].[Mangas] m ON uml.[MangaId] = m.[MangaId]
        LEFT JOIN [dbo].[MangasType] mt ON m.[MangaTypeId] = mt.[MangaTypeId]
        LEFT JOIN [dbo].[MangasDemographics] md ON m.[MangaId] = md.[MangaId]
        LEFT JOIN [dbo].[Demographics] d ON md.[DemographyId] = d.[DemographyId]
        WHERE 
            uml.[UserId] = @UserId
            AND (@ReadingStatus IS NULL OR uml.[ReadingStatus] = @ReadingStatus)
        ORDER BY uml.[CreatedAt] DESC;
    END

    -- =========================================================================
    -- ACTION 2: UPSERT (Añadir a la lista o actualizar estado de lectura)
    -- =========================================================================
    IF @Action = 2
    BEGIN
        -- Validar existencia del manga
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Mangas] WHERE [MangaId] = @MangaId)
        BEGIN
            SELECT 404 AS [Status], 'Manga not found' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            IF EXISTS (SELECT 1 FROM [dbo].[UserMangaLists] WHERE [UserId] = @UserId AND [MangaId] = @MangaId)
            BEGIN
                UPDATE [dbo].[UserMangaLists]
                SET [ReadingStatus] = ISNULL(@ReadingStatus, [ReadingStatus])
                WHERE [UserId] = @UserId AND [MangaId] = @MangaId;

                SELECT 200 AS [Status], 'Updated reading status' AS [ErrorDescription];
            END
            ELSE
            BEGIN
                INSERT INTO [dbo].[UserMangaLists] ([UserId], [MangaId], [ReadingStatus], [CreatedAt])
                VALUES (@UserId, @MangaId, ISNULL(@ReadingStatus, 'PlanToRead'), CURRENT_TIMESTAMP);

                SELECT 200 AS [Status], 'Added to user list' AS [ErrorDescription];
            END
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END

    -- =========================================================================
    -- ACTION 3: DELETE (Eliminar de la lista)
    -- =========================================================================
    IF @Action = 3
    BEGIN
        BEGIN TRY
            DELETE FROM [dbo].[UserMangaLists]
            WHERE [UserId] = @UserId AND [MangaId] = @MangaId;

            SELECT 200 AS [Status];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END

    -- =========================================================================
    -- ACTION 4: CHECK (Consultar si un manga está en la lista del usuario)
    -- =========================================================================
    IF @Action = 4
    BEGIN
        SELECT 
            uml.[UserListId],
            uml.[UserId],
            uml.[MangaId],
            uml.[ReadingStatus],
            uml.[CreatedAt] AS [AddedAt]
        FROM [dbo].[UserMangaLists] uml
        WHERE uml.[UserId] = @UserId AND uml.[MangaId] = @MangaId;
    END
END
RETURN 0;
