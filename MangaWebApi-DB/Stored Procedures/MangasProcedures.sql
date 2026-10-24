CREATE PROCEDURE [dbo].[MangasProcedures]
    @MangaId INT = NULL,
    @MangaTittle VARCHAR(255) = NULL,
    @MangaDescription VARCHAR(MAX) = NULL,
    @AlternativeNames VARCHAR(MAX) = NULL,
    @Mangaka VARCHAR(MAX) = NULL,
    @MangaFrontImgUrl VARCHAR(255) = NULL,
    @RegisteredBy INT = NULL,
    @MangaTypeId INT = NULL,
    @EditorialName VARCHAR(155) = NULL,
    @Rating DECIMAL(3, 1) = NULL,
    @Hidden BIT = NULL,
    @Action INT 
AS
BEGIN
    SET NOCOUNT ON;

    -- =========================================================================
    -- ACTION 1: SELECT (Buscar todos o por filtros adaptativos)
    -- =========================================================================
    IF @Action = 1
    BEGIN
        SELECT 
            [MangaId],
            [MangaTittle],
            [MangaDescription],
            [AlternativeNames],
            [Mangaka],
            [CreatedAt],
            [UpdatedAt],
            [MangaFrontImgUrl],
            [RegisteredBy],
            [MangaTypeId],
            [EditorialName],
            [Rating],
            [Hidden]
        FROM [dbo].[Mangas]
        WHERE 
            (@MangaId IS NULL OR [MangaId] = @MangaId)
            AND (@MangaTittle IS NULL OR [MangaTittle] LIKE '%' + @MangaTittle + '%')
            AND (@MangaTypeId IS NULL OR [MangaTypeId] = @MangaTypeId)
    END

    -- =========================================================================
    -- ACTION 2: INSERT (Crear manga con validación de título duplicado)
    -- =========================================================================
    IF @Action = 2
    BEGIN
        -- Validación: El título del manga ya existe
        IF EXISTS (SELECT 1 FROM [dbo].[Mangas] WHERE [MangaTittle] = TRIM(@MangaTittle))
        BEGIN
            SELECT 409 AS [Status], 'The manga title already exists' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            INSERT INTO [dbo].[Mangas] 
            (
                [MangaTittle], 
                [MangaDescription], 
                [AlternativeNames], 
                [Mangaka], 
                [CreatedAt], 
                [MangaFrontImgUrl], 
                [RegisteredBy], 
                [MangaTypeId], 
                [EditorialName], 
                [Rating], 
                [Hidden]
            )
            VALUES 
            (
                TRIM(@MangaTittle), 
                TRIM(@MangaDescription), 
                TRIM(@AlternativeNames), 
                TRIM(@Mangaka), 
                CURRENT_TIMESTAMP, 
                TRIM(@MangaFrontImgUrl), 
                @RegisteredBy, 
                @MangaTypeId, 
                TRIM(@EditorialName), 
                ISNULL(@Rating, 0.0), 
                ISNULL(@Hidden, 0)
            );

            SELECT 200 AS [Status], SCOPE_IDENTITY() AS [NewMangaId];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END

    -- =========================================================================
    -- ACTION 3: UPDATE (Actualizar campos específicos con validaciones)
    -- =========================================================================
    IF @Action = 3
    BEGIN
        -- Validación 1: Verificar si el registro existe
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Mangas] WHERE [MangaId] = @MangaId)
        BEGIN
            SELECT 404 AS [Status], 'Manga not found' AS [ErrorDescription];
            RETURN;
        END

        -- Validación 2: Si cambian el título, verificar que no cause duplicados en otros IDs
        IF @MangaTittle IS NOT NULL AND EXISTS (
            SELECT 1 FROM [dbo].[Mangas] 
            WHERE [MangaTittle] = TRIM(@MangaTittle) AND [MangaId] <> @MangaId
        )
        BEGIN
            SELECT 409 AS [Status], 'Another manga already has this title' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            UPDATE [dbo].[Mangas]
            SET 
                [MangaTittle] = ISNULL(TRIM(@MangaTittle), [MangaTittle]),
                [MangaDescription] = ISNULL(TRIM(@MangaDescription), [MangaDescription]),
                [AlternativeNames] = ISNULL(TRIM(@AlternativeNames), [AlternativeNames]),
                [Mangaka] = ISNULL(TRIM(@Mangaka), [Mangaka]),
                [UpdatedAt] = CURRENT_TIMESTAMP,
                [MangaFrontImgUrl] = ISNULL(TRIM(@MangaFrontImgUrl), [MangaFrontImgUrl]),
                [MangaTypeId] = ISNULL(@MangaTypeId, [MangaTypeId]),
                [EditorialName] = ISNULL(TRIM(@EditorialName), [EditorialName]),
                [Rating] = ISNULL(@Rating, [Rating]),
                [Hidden] = ISNULL(@Hidden, [Hidden])
            WHERE 
                [MangaId] = @MangaId;

            SELECT 200 AS [Status];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END
    
    -- =========================================================================
    -- ACTION 4: DELETE (Borrado físico con validación de existencia)
    -- =========================================================================
    IF @Action = 4
    BEGIN
        -- Validación: Verificar si el registro existe
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Mangas] WHERE [MangaId] = @MangaId)
        BEGIN
            SELECT 404 AS [Status], 'Manga not found' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            DELETE FROM [dbo].[Mangas]
            WHERE [MangaId] = @MangaId;

            SELECT 200 AS [Status];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END

END
RETURN 0;