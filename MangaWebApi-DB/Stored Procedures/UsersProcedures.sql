CREATE OR ALTER PROCEDURE [dbo].[UsersProcedures]
	@UserId INT = NULL,
	@Username VARCHAR(255) = NULL,
	@Email VARCHAR(255) = NULL,
	@Password VARCHAR(255) = NULL,
	@RoleId INT = NULL,
	@Status BIT = NULL,
	@CreatedAt DATETIME = NULL,
	@UpdatedAt DATETIME = NULL,
	@RecoveryToken VARCHAR(MAX) = NULL,
	@RecoveryTokenExpiration DATETIME = NULL,
	@AceptTerms BIT = NULL,
	@IsBlocked BIT = NULL,
	@ScanId INT = NULL,
	@Action INT
AS
BEGIN
	SET NOCOUNT ON;

	-- =========================================================================
	-- ACTION 1: SELECT (Buscar usuario por UserId, Email o Username)
	-- =========================================================================
	IF @Action = 1
	BEGIN
		SELECT 
			u.[UserId],
			u.[Username],
			u.[Email],
			u.[Password],
			u.[RoleId],
			r.[RoleName],
			r.[IsOwnerRole],
			u.[Status],
			u.[CreatedAt],
			u.[UpdatedAt],
			u.[RecoveryToken],
			u.[RecoveryTokenExpiration],
			u.[AceptTerms],
			u.[IsBlocked],
			u.[ScanId]
		FROM [dbo].[Users] u 
		LEFT JOIN [dbo].[Roles] r ON r.[RoleId] = u.[RoleId]
		WHERE 
			(@UserId IS NULL OR u.[UserId] = @UserId)
			AND (@Email IS NULL OR u.[Email] = @Email)
			AND (@Username IS NULL OR u.[Username] = @Username);
	END

	-- =========================================================================
	-- ACTION 2: INSERT / REGISTER (Registro de nuevo usuario)
	-- =========================================================================
	IF @Action = 2
	BEGIN
		IF EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Username] = TRIM(@Username))
		BEGIN
			SELECT 409 AS [Status], 'The username already exists' AS [ErrorDescription];
			RETURN;
		END

		IF EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Email] = TRIM(@Email))
		BEGIN
			SELECT 409 AS [Status], 'This email already exists' AS [ErrorDescription];
			RETURN;
		END

		BEGIN TRY
			INSERT INTO [dbo].[Users] 
			(
				[Username],
				[Email],
				[Password],
				[RoleId],
				[Status],
				[CreatedAt],
				[AceptTerms],
				[IsBlocked]
			)
			VALUES 
			(
				TRIM(@Username),
				TRIM(@Email),
				@Password,
				ISNULL(@RoleId, 1), -- 1: CLIENTE
				1,
				CURRENT_TIMESTAMP,
				ISNULL(@AceptTerms, 1),
				0
			);

			SELECT 200 AS [Status], SCOPE_IDENTITY() AS [NewUserId];
		END TRY
		BEGIN CATCH
			SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
		END CATCH
	END

	-- =========================================================================
	-- ACTION 3: SET RECOVERY TOKEN (Guardar token y fecha de expiración)
	-- =========================================================================
	IF @Action = 3
	BEGIN
		IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Email] = TRIM(@Email))
		BEGIN
			SELECT 404 AS [Status], 'No user found with this email' AS [ErrorDescription];
			RETURN;
		END

		BEGIN TRY
			UPDATE [dbo].[Users]
			SET 
				[RecoveryToken] = @RecoveryToken,
				[RecoveryTokenExpiration] = ISNULL(@RecoveryTokenExpiration, DATEADD(minute, 30, CURRENT_TIMESTAMP))
			WHERE [Email] = TRIM(@Email);

			SELECT 200 AS [Status];
		END TRY
		BEGIN CATCH
			SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
		END CATCH
	END

	-- =========================================================================
	-- ACTION 4: VALIDATE RECOVERY TOKEN (Comprobar vigencia del token)
	-- =========================================================================
	IF @Action = 4
	BEGIN
		DECLARE @FoundUserId INT = NULL;

		SELECT @FoundUserId = [UserId]
		FROM [dbo].[Users]
		WHERE 
			[Email] = TRIM(@Email)
			AND [RecoveryToken] = @RecoveryToken
			AND [RecoveryTokenExpiration] >= CURRENT_TIMESTAMP;

		IF @FoundUserId IS NOT NULL
		BEGIN
			SELECT 200 AS [Status], 'Token is valid' AS [ErrorDescription];
		END
		ELSE
		BEGIN
			SELECT 400 AS [Status], 'Invalid or expired recovery token' AS [ErrorDescription];
		END
	END

	-- =========================================================================
	-- ACTION 5: RESET PASSWORD (Restablecer contraseña usando token válido)
	-- =========================================================================
	IF @Action = 5
	BEGIN
		IF NOT EXISTS (
			SELECT 1 FROM [dbo].[Users] 
			WHERE [Email] = TRIM(@Email) 
			AND [RecoveryToken] = @RecoveryToken 
			AND [RecoveryTokenExpiration] >= CURRENT_TIMESTAMP
		)
		BEGIN
			SELECT 400 AS [Status], 'Invalid or expired recovery token' AS [ErrorDescription];
			RETURN;
		END

		BEGIN TRY
			UPDATE [dbo].[Users]
			SET 
				[Password] = @Password,
				[RecoveryToken] = NULL,
				[RecoveryTokenExpiration] = NULL,
				[UpdatedAt] = CURRENT_TIMESTAMP
			WHERE [Email] = TRIM(@Email);

			SELECT 200 AS [Status];
		END TRY
		BEGIN CATCH
			SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
		END CATCH
	END

	-- =========================================================================
	-- ACTION 6: CHANGE PASSWORD (Cambio de contraseña de usuario autenticado)
	-- =========================================================================
	IF @Action = 6
	BEGIN
		IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [UserId] = @UserId)
		BEGIN
			SELECT 404 AS [Status], 'User not found' AS [ErrorDescription];
			RETURN;
		END

		BEGIN TRY
			UPDATE [dbo].[Users]
			SET 
				[Password] = @Password,
				[UpdatedAt] = CURRENT_TIMESTAMP
			WHERE [UserId] = @UserId;

			SELECT 200 AS [Status];
		END TRY
		BEGIN CATCH
			SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
		END CATCH
	END

	-- =========================================================================
	-- ACTION 7: UPDATE PROFILE (Actualizar datos de perfil)
	-- =========================================================================
	IF @Action = 7
	BEGIN
		IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [UserId] = @UserId)
		BEGIN
			SELECT 404 AS [Status], 'User not found' AS [ErrorDescription];
			RETURN;
		END

		-- Validar si el nuevo username ya lo usa otra persona
		IF @Username IS NOT NULL AND EXISTS (
			SELECT 1 FROM [dbo].[Users] 
			WHERE [Username] = TRIM(@Username) AND [UserId] <> @UserId
		)
		BEGIN
			SELECT 409 AS [Status], 'This username is already taken by another account' AS [ErrorDescription];
			RETURN;
		END

		-- Validar si el nuevo email ya lo usa otra persona
		IF @Email IS NOT NULL AND EXISTS (
			SELECT 1 FROM [dbo].[Users] 
			WHERE [Email] = TRIM(@Email) AND [UserId] <> @UserId
		)
		BEGIN
			SELECT 409 AS [Status], 'This email is already registered by another account' AS [ErrorDescription];
			RETURN;
		END

		BEGIN TRY
			UPDATE [dbo].[Users]
			SET 
				[Username] = ISNULL(TRIM(@Username), [Username]),
				[Email] = ISNULL(TRIM(@Email), [Email]),
				[UpdatedAt] = CURRENT_TIMESTAMP
			WHERE [UserId] = @UserId;

			SELECT 200 AS [Status];
		END TRY
		BEGIN CATCH
			SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
		END CATCH
	END

END
RETURN 0;
