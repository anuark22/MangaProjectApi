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
    @Status VARCHAR(50) = NULL,
    @IsAmateur BIT = NULL,
    @IsErotic BIT = NULL,
    @IsFeatured BIT = NULL,
    @DemographyId INT = NULL,
    @Action INT 
AS
BEGIN
    SET NOCOUNT ON;

    -- =========================================================================
    -- ACTION 1: SELECT (Buscar todos o por filtros adaptativos para catálogo)
    -- =========================================================================
    IF @Action = 1
    BEGIN
        SELECT 
            m.[MangaId],
            m.[MangaTittle],
            m.[MangaDescription],
            m.[AlternativeNames],
            m.[Mangaka],
            m.[CreatedAt],
            m.[UpdatedAt],
            m.[MangaFrontImgUrl],
            m.[RegisteredBy],
            m.[MangaTypeId],
            ISNULL(mt.[MangaTypeName], '') AS [MangaTypeName],
            md.[DemographyId],
            ISNULL(d.[Name], '') AS [DemographyName],
            m.[EditorialName],
            m.[Rating],
            m.[Hidden],
            m.[Status],
            m.[IsAmateur],
            m.[IsErotic],
            m.[IsFeatured],
            m.[ViewCount]
        FROM [dbo].[Mangas] m
        LEFT JOIN [dbo].[MangasType] mt ON m.[MangaTypeId] = mt.[MangaTypeId]
        LEFT JOIN [dbo].[MangasDemographics] md ON m.[MangaId] = md.[MangaId]
        LEFT JOIN [dbo].[Demographics] d ON md.[DemographyId] = d.[DemographyId]
        WHERE 
            (@MangaId IS NULL OR m.[MangaId] = @MangaId)
            AND (@MangaTittle IS NULL OR m.[MangaTittle] LIKE '%' + @MangaTittle + '%')
            AND (@MangaTypeId IS NULL OR m.[MangaTypeId] = @MangaTypeId)
            AND (@DemographyId IS NULL OR md.[DemographyId] = @DemographyId)
            AND (@Status IS NULL OR m.[Status] = @Status)
            AND (@IsAmateur IS NULL OR m.[IsAmateur] = @IsAmateur)
            AND (@IsErotic IS NULL OR m.[IsErotic] = @IsErotic)
            AND (@IsFeatured IS NULL OR m.[IsFeatured] = @IsFeatured)
            AND (@Hidden IS NULL OR m.[Hidden] = @Hidden)
        ORDER BY m.[MangaId] DESC;
    END

    -- =========================================================================
    -- ACTION 2: INSERT (Crear manga con validación de título duplicado)
    -- =========================================================================
    IF @Action = 2
    BEGIN
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
                [Hidden],
                [Status],
                [IsAmateur],
                [IsErotic],
                [IsFeatured],
                [ViewCount]
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
                ISNULL(@Hidden, 0),
                ISNULL(@Status, 'Publishing'),
                ISNULL(@IsAmateur, 0),
                ISNULL(@IsErotic, 0),
                ISNULL(@IsFeatured, 0),
                0
            );

            DECLARE @NewMangaId INT = SCOPE_IDENTITY();

            IF @DemographyId IS NOT NULL
            BEGIN
                INSERT INTO [dbo].[MangasDemographics] ([DemographyId], [MangaId])
                VALUES (@DemographyId, @NewMangaId);
            END

            SELECT 200 AS [Status], @NewMangaId AS [NewMangaId];
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
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Mangas] WHERE [MangaId] = @MangaId)
        BEGIN
            SELECT 404 AS [Status], 'Manga not found' AS [ErrorDescription];
            RETURN;
        END

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
                [Hidden] = ISNULL(@Hidden, [Hidden]),
                [Status] = ISNULL(@Status, [Status]),
                [IsAmateur] = ISNULL(@IsAmateur, [IsAmateur]),
                [IsErotic] = ISNULL(@IsErotic, [IsErotic]),
                [IsFeatured] = ISNULL(@IsFeatured, [IsFeatured])
            WHERE 
                [MangaId] = @MangaId;

            IF @DemographyId IS NOT NULL
            BEGIN
                IF EXISTS (SELECT 1 FROM [dbo].[MangasDemographics] WHERE [MangaId] = @MangaId)
                BEGIN
                    UPDATE [dbo].[MangasDemographics]
                    SET [DemographyId] = @DemographyId
                    WHERE [MangaId] = @MangaId;
                END
                ELSE
                BEGIN
                    INSERT INTO [dbo].[MangasDemographics] ([DemographyId], [MangaId])
                    VALUES (@DemographyId, @MangaId);
                END
            END

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
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Mangas] WHERE [MangaId] = @MangaId)
        BEGIN
            SELECT 404 AS [Status], 'Manga not found' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            DELETE FROM [dbo].[MangasDemographics] WHERE [MangaId] = @MangaId;
            DELETE FROM [dbo].[MangaGenres] WHERE [MangaId] = @MangaId;
            DELETE FROM [dbo].[Mangas] WHERE [MangaId] = @MangaId;

            SELECT 200 AS [Status];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END

    -- =========================================================================
    -- ACTION 5: SELECT POPULAR (Para la sección Top Tier / Popular Mangas del Home)
    -- =========================================================================
    IF @Action = 5
    BEGIN
        SELECT TOP 10
            m.[MangaId],
            m.[MangaTittle],
            m.[MangaDescription],
            m.[AlternativeNames],
            m.[Mangaka],
            m.[CreatedAt],
            m.[UpdatedAt],
            m.[MangaFrontImgUrl],
            m.[RegisteredBy],
            m.[MangaTypeId],
            ISNULL(mt.[MangaTypeName], '') AS [MangaTypeName],
            md.[DemographyId],
            ISNULL(d.[Name], '') AS [DemographyName],
            m.[EditorialName],
            m.[Rating],
            m.[Hidden],
            m.[Status],
            m.[IsAmateur],
            m.[IsErotic],
            m.[IsFeatured],
            m.[ViewCount]
        FROM [dbo].[Mangas] m
        LEFT JOIN [dbo].[MangasType] mt ON m.[MangaTypeId] = mt.[MangaTypeId]
        LEFT JOIN [dbo].[MangasDemographics] md ON m.[MangaId] = md.[MangaId]
        LEFT JOIN [dbo].[Demographics] d ON md.[DemographyId] = d.[DemographyId]
        WHERE m.[Hidden] = 0
        ORDER BY m.[IsFeatured] DESC, m.[Rating] DESC, m.[ViewCount] DESC, m.[MangaId] DESC;
    END

END
RETURN 0;
