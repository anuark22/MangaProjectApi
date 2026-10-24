CREATE PROCEDURE [dbo].[GenresProcedures]
    @GenreId INT = NULL,
    @GenreName VARCHAR(255) = NULL,
    @GenreDescription VARCHAR(MAX) = NULL,
    @CreatedAt DATETIME = NULL,
    @Status BIT = NULL,
    @GenreImgUrl VARCHAR(255) = NULL,
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
            [GenreId],
            [GenreName],
            [GenreDescription],
            [CreatedAt],
            [Status],
            [GenreImgUrl],
            [UpdatedAt]
        FROM [dbo].[Genres]
        WHERE 
            (@GenreId IS NULL OR [GenreId] = @GenreId)
            AND (@GenreName IS NULL OR [GenreName] LIKE '%' + @GenreName + '%')
            AND Status = 1
    END

    -- =========================================================================
    -- ACTION 2: INSERT (Crear género con validación de nombre duplicado)
    -- =========================================================================
    IF @Action = 2
    BEGIN
        -- Validación: El nombre del género ya existe
        IF EXISTS (SELECT 1 FROM [dbo].[Genres] WHERE [GenreName] = TRIM(@GenreName))
        BEGIN
            SELECT 409 AS [Status], 'The genre name already exists' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            INSERT INTO [dbo].[Genres] 
            (
                [GenreName], 
                [GenreDescription], 
                [GenreImgUrl]
            )
            VALUES 
            (
                TRIM(@GenreName), 
                TRIM(@GenreDescription), 
                TRIM(@GenreImgUrl)
            );

            -- Devolvemos el status 200 y el ID recién creado en los items
            SELECT 200 AS [Status], SCOPE_IDENTITY() AS [NewGenreId];
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
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Genres] WHERE [GenreId] = @GenreId)
        BEGIN
            SELECT 404 AS [Status], 'Genre not found' AS [ErrorDescription];
            RETURN;
        END

        -- Validación 2: Si cambian el nombre, verificar que no cause duplicados en otros IDs
        IF @GenreName IS NOT NULL AND EXISTS (
            SELECT 1 FROM [dbo].[Genres] 
            WHERE [GenreName] = TRIM(@GenreName) AND [GenreId] <> @GenreId
        )
        BEGIN
            SELECT 409 AS [Status], 'Another genre already has this name' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            UPDATE [dbo].[Genres]
            SET 
                [GenreName] = ISNULL(TRIM(@GenreName), [GenreName]),
                [GenreDescription] = ISNULL(TRIM(@GenreDescription), [GenreDescription]),
                [Status] = ISNULL(@Status, [Status]),
                [GenreImgUrl] = ISNULL(TRIM(@GenreImgUrl), [GenreImgUrl]),
                [UpdatedAt] = CURRENT_TIMESTAMP
            WHERE 
                [GenreId] = @GenreId;

            SELECT 200 AS [Status];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END
    
    -- =========================================================================
    -- ACTION 4: DELETE (Inactivación lógica con validación de existencia)
    -- =========================================================================
    IF @Action = 4
    BEGIN
        -- Validación: Verificar si el registro existe
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Genres] WHERE [GenreId] = @GenreId)
        BEGIN
            SELECT 404 AS [Status], 'Genre not found' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            UPDATE [dbo].[Genres]
            SET 
                [Status] = 0
            WHERE 
                [GenreId] = @GenreId;

            SELECT 200 AS [Status];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END

END
RETURN 0;