SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[AuditLogsProcedures]
    @Action INT,
    @UserId INT = NULL,
    @Username NVARCHAR(150) = NULL,
    @UserRole NVARCHAR(50) = NULL,
    @ScanId INT = NULL,
    @AuditAction NVARCHAR(50) = NULL,
    @EntityType NVARCHAR(80) = NULL,
    @EntityId INT = NULL,
    @EntityName NVARCHAR(250) = NULL,
    @Details NVARCHAR(MAX) = NULL,
    @IpAddress NVARCHAR(64) = NULL,
    @Search NVARCHAR(200) = NULL,
    @Top INT = 150
AS
BEGIN
    SET NOCOUNT ON;

    -- Action 1: Insertar registro de auditoría
    IF @Action = 1
    BEGIN
        INSERT INTO [dbo].[AuditLogs]
        (
            [UserId],
            [Username],
            [UserRole],
            [ScanId],
            [Action],
            [EntityType],
            [EntityId],
            [EntityName],
            [Details],
            [IpAddress],
            [CreatedAt]
        )
        VALUES
        (
            @UserId,
            @Username,
            @UserRole,
            @ScanId,
            ISNULL(@AuditAction, 'ACTION'),
            ISNULL(@EntityType, 'System'),
            @EntityId,
            @EntityName,
            @Details,
            @IpAddress,
            GETUTCDATE()
        );

        SELECT SCOPE_IDENTITY() AS AuditId;
        RETURN;
    END

    -- Action 2: Consultar registros de auditoría con filtros
    IF @Action = 2
    BEGIN
        SELECT TOP (ISNULL(@Top, 150))
            a.[AuditId],
            a.[UserId],
            a.[Username],
            a.[UserRole],
            a.[ScanId],
            sg.[ScanName],
            a.[Action],
            a.[EntityType],
            a.[EntityId],
            a.[EntityName],
            a.[Details],
            a.[IpAddress],
            a.[CreatedAt]
        FROM [dbo].[AuditLogs] a
        LEFT JOIN [dbo].[ScansGroups] sg ON sg.[ScanId] = a.[ScanId]
        WHERE (@EntityType IS NULL OR @EntityType = '' OR a.[EntityType] = @EntityType)
          AND (@AuditAction IS NULL OR @AuditAction = '' OR a.[Action] = @AuditAction)
          AND (@UserId IS NULL OR a.[UserId] = @UserId)
          AND (
              @Search IS NULL OR @Search = '' OR
              a.[Username] LIKE '%' + @Search + '%' OR
              a.[EntityName] LIKE '%' + @Search + '%' OR
              a.[Details] LIKE '%' + @Search + '%' OR
              sg.[ScanName] LIKE '%' + @Search + '%'
          )
        ORDER BY a.[CreatedAt] DESC, a.[AuditId] DESC;
        RETURN;
    END
END
GO
