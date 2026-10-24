CREATE PROCEDURE [dbo].[DemographicsProcedures]
    @DemographyId INT = NULL,
    @Name VARCHAR(255) = NULL,
    @Description VARCHAR(MAX) = NULL,
    @DemographyImgUrl VARCHAR(MAX) = NULL,
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
            [DemographyId],
            [Name],
            [Description],
            [DemographyImgUrl]
        FROM [dbo].[Demographics]
        WHERE 
            (@DemographyId IS NULL OR [DemographyId] = @DemographyId)
            AND (@Name IS NULL OR [Name] LIKE '%' + @Name + '%')
    END

    -- =========================================================================
    -- ACTION 2: INSERT (Crear demografía con validación de nombre duplicado)
    -- =========================================================================
    IF @Action = 2
    BEGIN
        -- Validación: El nombre ya existe
        IF EXISTS (SELECT 1 FROM [dbo].[Demographics] WHERE [Name] = TRIM(@Name))
        BEGIN
            SELECT 409 AS [Status], 'The demographic name already exists' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            INSERT INTO [dbo].[Demographics] 
            (
                [Name], 
                [Description], 
                [DemographyImgUrl]
            )
            VALUES 
            (
                TRIM(@Name), 
                TRIM(@Description), 
                TRIM(@DemographyImgUrl)
            );

            -- Devolvemos el status 200 y el ID recién creado
            SELECT 200 AS [Status], SCOPE_IDENTITY() AS [NewDemographyId];
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
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Demographics] WHERE [DemographyId] = @DemographyId)
        BEGIN
            SELECT 404 AS [Status], 'Demographic not found' AS [ErrorDescription];
            RETURN;
        END

        -- Validación 2: Si cambian el nombre, verificar que no cause duplicados en otros IDs
        IF @Name IS NOT NULL AND EXISTS (
            SELECT 1 FROM [dbo].[Demographics] 
            WHERE [Name] = TRIM(@Name) AND [DemographyId] <> @DemographyId
        )
        BEGIN
            SELECT 409 AS [Status], 'Another demographic already has this name' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            UPDATE [dbo].[Demographics]
            SET 
                [Name] = ISNULL(TRIM(@Name), [Name]),
                [Description] = ISNULL(TRIM(@Description), [Description]),
                [DemographyImgUrl] = ISNULL(TRIM(@DemographyImgUrl), [DemographyImgUrl])
            WHERE 
                [DemographyId] = @DemographyId;

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
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Demographics] WHERE [DemographyId] = @DemographyId)
        BEGIN
            SELECT 404 AS [Status], 'Demographic not found' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            DELETE FROM [dbo].[Demographics]
            WHERE [DemographyId] = @DemographyId;

            SELECT 200 AS [Status];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END

END
RETURN 0;