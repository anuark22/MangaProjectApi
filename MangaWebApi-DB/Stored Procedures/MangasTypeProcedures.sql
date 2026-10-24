CREATE PROCEDURE [dbo].[MangasTypeProcedures]
    @MangaTypeId INT = NULL,
    @MangaTypeName VARCHAR(255) = NULL,
    @MangaTypeImgUrl VARCHAR(255) = NULL,
    @Status BIT = NULL,
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
            [MangaTypeId],
            [MangaTypeName],
            [MangaTypeImgUrl],
            [Status]
        FROM [dbo].[MangasType]
        WHERE 
            (@MangaTypeId IS NULL OR [MangaTypeId] = @MangaTypeId)
            AND (@MangaTypeName IS NULL OR [MangaTypeName] LIKE '%' + @MangaTypeName + '%')
    END

    -- =========================================================================
    -- ACTION 2: INSERT (Crear tipo de manga con validación de nombre duplicado)
    -- =========================================================================
    IF @Action = 2
    BEGIN
        -- Validación: El nombre ya existe
        IF EXISTS (SELECT 1 FROM [dbo].[MangasType] WHERE [MangaTypeName] = TRIM(@MangaTypeName))
        BEGIN
            SELECT 409 AS [Status], 'The manga type name already exists' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            INSERT INTO [dbo].[MangasType] 
            (
                [MangaTypeName], 
                [MangaTypeImgUrl], 
                [Status]
            )
            VALUES 
            (
                TRIM(@MangaTypeName), 
                TRIM(@MangaTypeImgUrl), 
                ISNULL(@Status, 1)
            );

            SELECT 200 AS [Status], SCOPE_IDENTITY() AS [NewMangaTypeId];
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
        IF NOT EXISTS (SELECT 1 FROM [dbo].[MangasType] WHERE [MangaTypeId] = @MangaTypeId)
        BEGIN
            SELECT 404 AS [Status], 'Manga type not found' AS [ErrorDescription];
            RETURN;
        END

        -- Validación 2: Si cambian el nombre, verificar que no cause duplicados en otros IDs
        IF @MangaTypeName IS NOT NULL AND EXISTS (
            SELECT 1 FROM [dbo].[MangasType] 
            WHERE [MangaTypeName] = TRIM(@MangaTypeName) AND [MangaTypeId] <> @MangaTypeId
        )
        BEGIN
            SELECT 409 AS [Status], 'Another manga type already has this name' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            UPDATE [dbo].[MangasType]
            SET 
                [MangaTypeName] = ISNULL(TRIM(@MangaTypeName), [MangaTypeName]),
                [MangaTypeImgUrl] = ISNULL(TRIM(@MangaTypeImgUrl), [MangaTypeImgUrl]),
                [Status] = ISNULL(@Status, [Status])
            WHERE 
                [MangaTypeId] = @MangaTypeId;

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
        IF NOT EXISTS (SELECT 1 FROM [dbo].[MangasType] WHERE [MangaTypeId] = @MangaTypeId)
        BEGIN
            SELECT 404 AS [Status], 'Manga type not found' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            DELETE FROM [dbo].[MangasType]
            WHERE [MangaTypeId] = @MangaTypeId;

            SELECT 200 AS [Status];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END

END
Return 0;