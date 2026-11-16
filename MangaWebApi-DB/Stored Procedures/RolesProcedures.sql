CREATE OR ALTER PROCEDURE [dbo].[RolesProcedures]
    @RoleId INT = NULL,
    @RoleName VARCHAR(45) = NULL,
    @RoleDescription VARCHAR(100) = NULL,
    @IsActive TINYINT = NULL,
    @IsOwnerRole TINYINT = NULL,
    @UserId INT = NULL,
    @Action INT
AS
BEGIN
    SET NOCOUNT ON;

    -- =========================================================================
    -- ACTION 1: SELECT (Listar roles o filtrar por ID, Nombre o Estado)
    -- =========================================================================
    IF @Action = 1
    BEGIN
        SELECT 
            r.[RoleId],
            r.[RoleName],
            r.[RoleDescription],
            r.[IsActive],
            r.[IsOwnerRole],
            r.[IsDefaultRole],
            (SELECT COUNT(1) FROM [dbo].[Users] u WHERE u.[RoleId] = r.[RoleId]) AS [UserCount]
        FROM [dbo].[Roles] r
        WHERE 
            (@RoleId IS NULL OR r.[RoleId] = @RoleId)
            AND (@RoleName IS NULL OR r.[RoleName] LIKE '%' + @RoleName + '%')
            AND (@IsActive IS NULL OR r.[IsActive] = @IsActive)
        ORDER BY r.[IsDefaultRole] DESC, r.[RoleId] ASC;
    END

    -- =========================================================================
    -- ACTION 2: INSERT (Crear nuevo rol - nunca es default del sistema)
    -- =========================================================================
    IF @Action = 2
    BEGIN
        IF EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE [RoleName] = TRIM(@RoleName))
        BEGIN
            SELECT 409 AS [Status], 'The role name already exists' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            INSERT INTO [dbo].[Roles]
            (
                [RoleName],
                [RoleDescription],
                [IsActive],
                [IsOwnerRole],
                [IsDefaultRole]
            )
            VALUES
            (
                TRIM(@RoleName),
                TRIM(@RoleDescription),
                ISNULL(@IsActive, 1),
                ISNULL(@IsOwnerRole, 0),
                0 -- Roles creados por el usuario siempre tienen IsDefaultRole = 0
            );

            SELECT 200 AS [Status], SCOPE_IDENTITY() AS [NewRoleId];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END

    -- =========================================================================
    -- ACTION 3: UPDATE (Actualizar rol existente)
    -- =========================================================================
    IF @Action = 3
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE [RoleId] = @RoleId)
        BEGIN
            SELECT 404 AS [Status], 'Role not found' AS [ErrorDescription];
            RETURN;
        END

        IF @RoleName IS NOT NULL AND EXISTS (
            SELECT 1 FROM [dbo].[Roles] 
            WHERE [RoleName] = TRIM(@RoleName) AND [RoleId] <> @RoleId
        )
        BEGIN
            SELECT 409 AS [Status], 'Another role already has this name' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            UPDATE [dbo].[Roles]
            SET 
                [RoleName] = ISNULL(TRIM(@RoleName), [RoleName]),
                [RoleDescription] = ISNULL(TRIM(@RoleDescription), [RoleDescription]),
                [IsActive] = ISNULL(@IsActive, [IsActive])
            WHERE [RoleId] = @RoleId;

            SELECT 200 AS [Status];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END

    -- =========================================================================
    -- ACTION 4: DELETE (Eliminar rol - SOLO si NO es por defecto y NO tiene usuarios)
    -- =========================================================================
    IF @Action = 4
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE [RoleId] = @RoleId)
        BEGIN
            SELECT 404 AS [Status], 'Role not found' AS [ErrorDescription];
            RETURN;
        END

        -- 1. Regla crítica: No permitir eliminar roles por defecto del sistema
        IF (SELECT [IsDefaultRole] FROM [dbo].[Roles] WHERE [RoleId] = @RoleId) = 1
        BEGIN
            SELECT 403 AS [Status], 'Los roles creados por defecto del sistema no pueden ser eliminados' AS [ErrorDescription];
            RETURN;
        END

        -- 2. Regla de integridad: No permitir eliminar roles con usuarios asignados
        IF EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [RoleId] = @RoleId)
        BEGIN
            SELECT 409 AS [Status], 'No se puede eliminar el rol porque tiene usuarios asignados. Reasigna los usuarios a otro rol primero' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            DELETE FROM [dbo].[Roles] WHERE [RoleId] = @RoleId;
            SELECT 200 AS [Status];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END

    -- =========================================================================
    -- ACTION 5: ASSIGN ROLE TO USER (Asignar rol a un usuario)
    -- =========================================================================
    IF @Action = 5
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [UserId] = @UserId)
        BEGIN
            SELECT 404 AS [Status], 'User not found' AS [ErrorDescription];
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE [RoleId] = @RoleId)
        BEGIN
            SELECT 404 AS [Status], 'Role not found' AS [ErrorDescription];
            RETURN;
        END

        IF (SELECT [IsActive] FROM [dbo].[Roles] WHERE [RoleId] = @RoleId) = 0
        BEGIN
            SELECT 400 AS [Status], 'No se puede asignar un rol inactivo' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            UPDATE [dbo].[Users]
            SET 
                [RoleId] = @RoleId,
                [UpdatedAt] = CURRENT_TIMESTAMP
            WHERE [UserId] = @UserId;

            SELECT 200 AS [Status];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
    END

    -- =========================================================================
    -- ACTION 6: GET USERS BY ROLE (Consultar usuarios que tienen un rol)
    -- =========================================================================
    IF @Action = 6
    BEGIN
        SELECT 
            u.[UserId],
            u.[Username],
            u.[Email],
            u.[RoleId],
            r.[RoleName],
            u.[Status],
            u.[CreatedAt]
        FROM [dbo].[Users] u
        INNER JOIN [dbo].[Roles] r ON u.[RoleId] = r.[RoleId]
        WHERE u.[RoleId] = @RoleId
        ORDER BY u.[Username] ASC;
    END

END
RETURN 0;
