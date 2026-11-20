CREATE PROCEDURE [dbo].[LiveChatMessagesProcedures]
    @Action INT,
    @MessageId NVARCHAR(32) = NULL,
    @UserId INT = NULL,
    @Username NVARCHAR(150) = NULL,
    @Email NVARCHAR(255) = NULL,
    @RoleId INT = NULL,
    @Text NVARCHAR(500) = NULL,
    @MediaUrl NVARCHAR(1000) = NULL,
    @CreatedAt DATETIME2 = NULL,
    @Top INT = 60
AS
BEGIN
    SET NOCOUNT ON;

    -- Action 1: Guardar mensaje nuevo
    IF @Action = 1
    BEGIN
        INSERT INTO [dbo].[LiveChatMessages]
        (
            [MessageId],
            [UserId],
            [Username],
            [Email],
            [RoleId],
            [Text],
            [MediaUrl],
            [CreatedAt]
        )
        VALUES
        (
            @MessageId,
            ISNULL(@UserId, 0),
            ISNULL(@Username, 'Lector'),
            ISNULL(@Email, ''),
            ISNULL(@RoleId, 1),
            ISNULL(@Text, ''),
            @MediaUrl,
            ISNULL(@CreatedAt, SYSUTCDATETIME())
        );
        RETURN;
    END

    -- Action 2: Obtener últimos N mensajes en orden cronológico (más antiguo -> más reciente)
    IF @Action = 2
    BEGIN
        SELECT
            [Id],
            [UserId],
            [Username],
            [Email],
            [RoleId],
            [Text],
            [MediaUrl],
            [CreatedAt]
        FROM (
            SELECT TOP (ISNULL(@Top, 60))
                [MessageId] AS [Id],
                [UserId],
                [Username],
                [Email],
                [RoleId],
                [Text],
                [MediaUrl],
                [CreatedAt]
            FROM [dbo].[LiveChatMessages]
            ORDER BY [CreatedAt] DESC
        ) recent
        ORDER BY [CreatedAt] ASC;
        RETURN;
    END
END
GO
