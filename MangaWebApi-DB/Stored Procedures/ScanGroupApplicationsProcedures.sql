CREATE OR ALTER PROCEDURE [dbo].[ScanGroupApplicationsProcedures]
    @ApplicationId INT = NULL,
    @UserId INT = NULL,
    @ScanName VARCHAR(255) = NULL,
    @ScanDescription VARCHAR(MAX) = NULL,
    @ScanImgUrl VARCHAR(255) = NULL,
    @EmailNotification BIT = NULL,
    @DiscordUrl VARCHAR(255) = NULL,
    @WebsiteUrl VARCHAR(255) = NULL,
    @ApplicationMessage VARCHAR(MAX) = NULL,
    @Status VARCHAR(20) = NULL,
    @AdminNotes VARCHAR(MAX) = NULL,
    @AdminUserId INT = NULL,
    @Action INT
AS
BEGIN
    SET NOCOUNT ON;

    -- =========================================================================
    -- ACTION 1: SELECT (Listado o detalle para Admin)
    -- =========================================================================
    IF @Action = 1
    BEGIN
        SELECT 
            A.[ApplicationId],
            A.[UserId],
            U.[Username],
            U.[Email] AS [UserEmail],
            A.[ScanName],
            A.[ScanDescription],
            A.[ScanImgUrl],
            A.[EmailNotification],
            A.[DiscordUrl],
            A.[WebsiteUrl],
            A.[ApplicationMessage],
            A.[Status],
            A.[AdminNotes],
            A.[ReviewedBy],
            R.[Username] AS [ReviewedByUsername],
            A.[ReviewedAt],
            A.[CreatedScanId],
            A.[CreatedAt],
            A.[UpdatedAt]
        FROM [dbo].[ScanGroupApplications] A
        INNER JOIN [dbo].[Users] U ON A.[UserId] = U.[UserId]
        LEFT JOIN [dbo].[Users] R ON A.[ReviewedBy] = R.[UserId]
        WHERE 
            (@ApplicationId IS NULL OR A.[ApplicationId] = @ApplicationId)
            AND (@Status IS NULL OR A.[Status] = @Status)
            AND (@UserId IS NULL OR A.[UserId] = @UserId)
        ORDER BY A.[CreatedAt] DESC;
        RETURN;
    END

    -- =========================================================================
    -- ACTION 2: INSERT / APPLY (Usuario solicita convertirse en Scan Group)
    -- =========================================================================
    IF @Action = 2
    BEGIN
        -- 1. Validar que el usuario exista
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [UserId] = @UserId)
        BEGIN
            SELECT 404 AS [Status], 'Usuario no encontrado' AS [ErrorDescription];
            RETURN;
        END

        -- 2. Validar que el usuario no sea ya Scan Group (RoleId = 3)
        IF EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [UserId] = @UserId AND [RoleId] = 3)
        BEGIN
            SELECT 400 AS [Status], 'Ya tienes asignado el rol de Scan Group.' AS [ErrorDescription];
            RETURN;
        END

        -- 3. Validar que no tenga ya una solicitud pendiente
        IF EXISTS (SELECT 1 FROM [dbo].[ScanGroupApplications] WHERE [UserId] = @UserId AND [Status] = 'PENDING')
        BEGIN
            SELECT 400 AS [Status], 'Ya tienes una solicitud de Scan Group pendiente de revisión.' AS [ErrorDescription];
            RETURN;
        END

        -- 4. Validar que el nombre del scan no esté registrado ya en ScansGroups
        IF EXISTS (SELECT 1 FROM [dbo].[ScansGroups] WHERE [ScanName] = TRIM(@ScanName))
        BEGIN
            SELECT 409 AS [Status], 'Ya existe un grupo de scan con este nombre.' AS [ErrorDescription];
            RETURN;
        END

        -- 5. Validar que el nombre del scan no esté en otra solicitud pendiente
        IF EXISTS (SELECT 1 FROM [dbo].[ScanGroupApplications] WHERE [ScanName] = TRIM(@ScanName) AND [Status] = 'PENDING')
        BEGIN
            SELECT 409 AS [Status], 'Ya existe una solicitud pendiente con este nombre de grupo.' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            INSERT INTO [dbo].[ScanGroupApplications]
            (
                [UserId],
                [ScanName],
                [ScanDescription],
                [ScanImgUrl],
                [EmailNotification],
                [DiscordUrl],
                [WebsiteUrl],
                [ApplicationMessage],
                [Status]
            )
            VALUES
            (
                @UserId,
                TRIM(@ScanName),
                TRIM(@ScanDescription),
                TRIM(@ScanImgUrl),
                ISNULL(@EmailNotification, 0),
                TRIM(@DiscordUrl),
                TRIM(@WebsiteUrl),
                TRIM(@ApplicationMessage),
                'PENDING'
            );

            SELECT 200 AS [Status], SCOPE_IDENTITY() AS [NewApplicationId], 'Solicitud enviada exitosamente. Un administrador la revisará.' AS [ErrorDescription];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
        RETURN;
    END

    -- =========================================================================
    -- ACTION 3: APPROVE (Admin aprueba solicitud, crea Scan Group y actualiza rol)
    -- =========================================================================
    IF @Action = 3
    BEGIN
        -- 1. Validar que la solicitud exista
        IF NOT EXISTS (SELECT 1 FROM [dbo].[ScanGroupApplications] WHERE [ApplicationId] = @ApplicationId)
        BEGIN
            SELECT 404 AS [Status], 'Solicitud no encontrada' AS [ErrorDescription];
            RETURN;
        END

        -- 2. Validar que esté en estado PENDING
        IF (SELECT [Status] FROM [dbo].[ScanGroupApplications] WHERE [ApplicationId] = @ApplicationId) <> 'PENDING'
        BEGIN
            SELECT 400 AS [Status], 'Esta solicitud ya ha sido procesada previamente.' AS [ErrorDescription];
            RETURN;
        END

        DECLARE @AppUserId INT;
        DECLARE @AppScanName VARCHAR(255);
        DECLARE @AppDesc VARCHAR(MAX);
        DECLARE @AppImg VARCHAR(255);
        DECLARE @AppEmailNotif BIT;

        SELECT 
            @AppUserId = [UserId],
            @AppScanName = [ScanName],
            @AppDesc = [ScanDescription],
            @AppImg = [ScanImgUrl],
            @AppEmailNotif = [EmailNotification]
        FROM [dbo].[ScanGroupApplications]
        WHERE [ApplicationId] = @ApplicationId;

        -- 3. Validar que el nombre del grupo no se haya ocupado mientras tanto
        IF EXISTS (SELECT 1 FROM [dbo].[ScansGroups] WHERE [ScanName] = @AppScanName)
        BEGIN
            SELECT 409 AS [Status], 'El nombre del grupo ya fue registrado por otro scan.' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            BEGIN TRANSACTION;

            -- A) Crear el nuevo Scan Group
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
                @AppScanName,
                @AppDesc,
                @AppUserId,
                ISNULL(@AppEmailNotif, 0),
                @AppImg,
                1
            );

            DECLARE @NewScanId INT = SCOPE_IDENTITY();

            -- B) Actualizar el rol del usuario a SCAN (RoleId = 3), preservando si ya es ADMIN
            UPDATE [dbo].[Users]
            SET 
                [RoleId] = CASE WHEN [RoleId] = 2 THEN 2 ELSE 3 END,
                [UpdatedAt] = CURRENT_TIMESTAMP
            WHERE [UserId] = @AppUserId;

            -- C) Actualizar estado de la solicitud a APPROVED
            UPDATE [dbo].[ScanGroupApplications]
            SET 
                [Status] = 'APPROVED',
                [CreatedScanId] = @NewScanId,
                [ReviewedBy] = @AdminUserId,
                [ReviewedAt] = CURRENT_TIMESTAMP,
                [AdminNotes] = @AdminNotes,
                [UpdatedAt] = CURRENT_TIMESTAMP
            WHERE [ApplicationId] = @ApplicationId;

            COMMIT TRANSACTION;

            SELECT 200 AS [Status], 'Solicitud aprobada exitosamente. Grupo de scan creado y rol actualizado.' AS [ErrorDescription];
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;

            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
        RETURN;
    END

    -- =========================================================================
    -- ACTION 4: REJECT (Admin rechaza la solicitud)
    -- =========================================================================
    IF @Action = 4
    BEGIN
        -- 1. Validar que la solicitud exista
        IF NOT EXISTS (SELECT 1 FROM [dbo].[ScanGroupApplications] WHERE [ApplicationId] = @ApplicationId)
        BEGIN
            SELECT 404 AS [Status], 'Solicitud no encontrada' AS [ErrorDescription];
            RETURN;
        END

        -- 2. Validar que esté en estado PENDING
        IF (SELECT [Status] FROM [dbo].[ScanGroupApplications] WHERE [ApplicationId] = @ApplicationId) <> 'PENDING'
        BEGIN
            SELECT 400 AS [Status], 'Esta solicitud ya ha sido procesada previamente.' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            UPDATE [dbo].[ScanGroupApplications]
            SET 
                [Status] = 'REJECTED',
                [ReviewedBy] = @AdminUserId,
                [ReviewedAt] = CURRENT_TIMESTAMP,
                [AdminNotes] = @AdminNotes,
                [UpdatedAt] = CURRENT_TIMESTAMP
            WHERE [ApplicationId] = @ApplicationId;

            SELECT 200 AS [Status], 'Solicitud rechazada correctamente.' AS [ErrorDescription];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
        RETURN;
    END

    -- =========================================================================
    -- ACTION 5: GET MY APPLICATION (Consultar la solicitud más reciente del usuario)
    -- =========================================================================
    IF @Action = 5
    BEGIN
        SELECT TOP 1
            A.[ApplicationId],
            A.[UserId],
            U.[Username],
            U.[Email] AS [UserEmail],
            A.[ScanName],
            A.[ScanDescription],
            A.[ScanImgUrl],
            A.[EmailNotification],
            A.[DiscordUrl],
            A.[WebsiteUrl],
            A.[ApplicationMessage],
            A.[Status],
            A.[AdminNotes],
            A.[ReviewedBy],
            R.[Username] AS [ReviewedByUsername],
            A.[ReviewedAt],
            A.[CreatedScanId],
            A.[CreatedAt],
            A.[UpdatedAt]
        FROM [dbo].[ScanGroupApplications] A
        INNER JOIN [dbo].[Users] U ON A.[UserId] = U.[UserId]
        LEFT JOIN [dbo].[Users] R ON A.[ReviewedBy] = R.[UserId]
        WHERE A.[UserId] = @UserId
        ORDER BY A.[CreatedAt] DESC;
        RETURN;
    END

END;
