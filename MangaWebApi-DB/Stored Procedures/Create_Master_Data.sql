CREATE PROCEDURE [dbo].[Create_Master_Data]
AS
BEGIN
	IF NOT EXISTS(SELECT * FROM Roles WHERE RoleId = 1)
	BEGIN
		SET IDENTITY_INSERT Roles ON
		INSERT INTO Roles (RoleId, RoleName, RoleDescription, IsActive, IsOwnerRole)
		VALUES (1, 'CLIENTE', 'Role de Cliente Default', 1, 0);
		SET IDENTITY_INSERT Roles OFF
	END
	ELSE 
		UPDATE Roles SET 
			RoleName = 'CLIENTE', 
			RoleDescription = 'Role de Cliente Default',
			IsActive = 1 
		WHERE RoleId = 1
	IF NOT EXISTS(SELECT * FROM Roles WHERE RoleId = 2)
	BEGIN
		SET IDENTITY_INSERT Roles ON
		INSERT INTO Roles (RoleId, RoleName, RoleDescription, IsActive, IsOwnerRole)
		VALUES (2, 'ADMIN', 'Role de Administración Default', 1, 0);
		SET IDENTITY_INSERT Roles OFF
	END
	ELSE 
		UPDATE Roles SET 
			RoleName = 'ADMIN', 
			RoleDescription = 'Role de Administración Default',
			IsActive = 1 
		WHERE RoleId = 2
END
