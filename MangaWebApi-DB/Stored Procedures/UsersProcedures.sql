CREATE PROCEDURE [dbo].[UsersProcedures]
	@UserId INT = NULL,
	@Username VARCHAR(255) = NULL,
	@Email VARCHAR(255) = NULL,
	@Password VARCHAR(255) = NULL,
	@RoleId INT = NULL,
	@Status BIT = NULL,
	@CreatedAt DATETIME = NULL,
	@UpdatedAt DATETIME = NULL,
	@RecoveryToken VARCHAR(MAX) = NULL,
	@AceptTerms BIT = NULL,
	@IsBlocked BIT = NULL,
	@ScanId INT = NULL,
	@Action INT
AS
	IF @Action = 1
	BEGIN
		SELECT u.*,
			   r.RoleId,
			   r.RoleName,
			   r.IsOwnerRole
		FROM Users u 
		LEFT JOIN Roles r ON r.RoleId = u.RoleId
		WHERE @UserId IS NULL OR (@UserId = UserId)
	END
	IF @Action = 2
	BEGIN
		IF EXISTS (SELECT * FROM Users WHERE Username = @Username)
		BEGIN
			SELECT 409 [Status], 'The username already exist' [ErrorDescription]
			RETURN
		END
		DECLARE @NameFromDb VARCHAR(45) = (SELECT Email FROM Users WHERE Email=@Email);
		IF @NameFromDb IS NULL
		BEGIN
			INSERT INTO Users (Username,Email,[Password],RoleId,CreatedAt,AceptTerms)
			VALUES (@Username,@Email,@Password,1,CURRENT_TIMESTAMP,@AceptTerms);
			SELECT 200 [Status]
		END
		ELSE
		BEGIN
			SELECT 409 [Status], 'This email already exist' [ErrorDescription]
		END
	END
RETURN 0
