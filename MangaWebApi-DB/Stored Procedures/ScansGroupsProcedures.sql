CREATE PROCEDURE [dbo].[ScansGroupsProcedures]
    @ScanId INT = NULL,
    @ScanName VARCHAR(255) = NULL,
    @ScanDescription VARCHAR(MAX) = NULL,
    @CreatedBy INT = NULL,
    @EmailNotificaction BIT = NULL,
    @ScanImgUrl VARCHAR(255) = NULL,
    @ScanStatus BIT = NULL,
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
            [ScanId],
            [ScanName],
            [ScanDescription],
            [CreatedBy],
            [EmailNotificaction],
            [ScanImgUrl],
            [ScanStatus]
        FROM [dbo].[ScansGroups]
        WHERE 
            (@ScanId IS NULL OR [ScanId] = @ScanId)
            AND (@ScanName IS NULL OR [ScanName] LIKE '%' + @ScanName + '%')
    END

    -- =========================================================================
    -- ACTION 2: INSERT (Crear grupo de scan con validación de nombre duplicado)
    -- =========================================================================
    IF @Action = 2
    BEGIN
        -- Validación: El nombre ya existe
        IF EXISTS (SELECT 1 FROM [dbo].[ScansGroups] WHERE [ScanName] = TRIM(@ScanName))
        BEGIN
            SELECT 409 AS [Status], 'The scan group name already exists' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            INSERT INTO [dbo].[ScansGroups] 
            (
                [ScanName], 
                [ScanDescription], 
                [CreatedBy], 
                [EmailNotificaction], 
                [ScanImgUrl], 
                [ScanStatus]
            )
            VALUES 
            (
                TRIM(@ScanName), 
                TRIM(@ScanDescription), 
                @CreatedBy, 
                ISNULL(@EmailNotificaction, 0), 
                TRIM(@ScanImgUrl), 
                ISNULL(@ScanStatus, 1)
            );

            SELECT 200 AS [Status], SCOPE_IDENTITY() AS [NewScanId];
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
        IF NOT EXISTS (SELECT 1 FROM [dbo].[ScansGroups] WHERE [ScanId] = @ScanId)
        BEGIN
            SELECT 404 AS [Status], 'Scan group not found' AS [ErrorDescription];
            RETURN;
        END

        -- Validación 2: Si cambian el nombre, verificar que no cause duplicados en otros IDs
        IF @ScanName IS NOT NULL AND EXISTS (
            SELECT 1 FROM [dbo].[ScansGroups] 
            WHERE [ScanName] = TRIM(@ScanName) AND [ScanId] <> @ScanId
        )
        BEGIN
            SELECT 409 AS [Status], 'Another scan group already has this name' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            UPDATE [dbo].[ScansGroups]
            SET 
                [ScanName] = ISNULL(TRIM(@ScanName), [ScanName]),
                [ScanDescription] = ISNULL(TRIM(@ScanDescription), [ScanDescription]),
                [CreatedBy] = ISNULL(@CreatedBy, [CreatedBy]),
                [EmailNotificaction] = ISNULL(@EmailNotificaction, [EmailNotificaction]),
                [ScanImgUrl] = ISNULL(TRIM(@ScanImgUrl), [ScanImgUrl]),
                [ScanStatus] = ISNULL(@ScanStatus, [ScanStatus])
            WHERE 
                [ScanId] = @ScanId;

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
        IF NOT EXISTS (SELECT 1 FROM [dbo].[ScansGroups] WHERE [ScanId] = @ScanId)
        BEGIN
            SELECT 404 AS [Status], 'Scan group not found' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            DELETE FROM [dbo].[ScansGroups]
            WHERE [ScanId] = @ScanId;

            SELECT 200 AS [Status];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END

END
RETURN 0;