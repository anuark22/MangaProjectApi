SET QUOTED_IDENTIFIER ON;
GO
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[ChapterReportsProcedures]
    @ReportId INT = NULL,
    @ChapterId INT = NULL,
    @UserId INT = NULL,
    @MangaId INT = NULL,
    @ScanId INT = NULL,
    @Reason VARCHAR(100) = NULL,
    @Description VARCHAR(1000) = NULL,
    @Status VARCHAR(20) = NULL,
    @AdminNotes VARCHAR(1000) = NULL,
    @ActionTaken VARCHAR(100) = NULL,
    @ReviewedBy INT = NULL,
    @Action INT
AS
BEGIN
    SET NOCOUNT ON;

    -- =========================================================================
    -- ACTION 1: SELECT Reports (Para Panel de Administración)
    -- =========================================================================
    IF @Action = 1
    BEGIN
        SELECT
            cr.[ReportId],
            cr.[ChapterId],
            cr.[ChapterNumber],
            cr.[ChapterTitle],
            cr.[MangaId],
            m.[MangaTittle],
            cr.[ScanId],
            sg.[ScanName],
            cr.[UserId],
            u.[Username] AS [ReporterUsername],
            u.[Email] AS [ReporterEmail],
            cr.[Reason],
            cr.[Description],
            cr.[Status],
            cr.[AdminNotes],
            cr.[ActionTaken],
            cr.[ReviewedBy],
            rev.[Username] AS [ReviewerUsername],
            cr.[ReviewedAt],
            cr.[CreatedAt],
            cr.[UpdatedAt],
            CAST(CASE WHEN mc.[ChapterId] IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS [ChapterExists]
        FROM [dbo].[ChapterReports] cr
        INNER JOIN [dbo].[Mangas] m ON cr.[MangaId] = m.[MangaId]
        INNER JOIN [dbo].[ScansGroups] sg ON cr.[ScanId] = sg.[ScanId]
        INNER JOIN [dbo].[Users] u ON cr.[UserId] = u.[UserId]
        LEFT JOIN [dbo].[Users] rev ON cr.[ReviewedBy] = rev.[UserId]
        LEFT JOIN [dbo].[MangaChapters] mc ON cr.[ChapterId] = mc.[ChapterId]
        WHERE
            (@ReportId IS NULL OR cr.[ReportId] = @ReportId)
            AND (@Status IS NULL OR cr.[Status] = @Status)
            AND (@MangaId IS NULL OR cr.[MangaId] = @MangaId)
            AND (@ScanId IS NULL OR cr.[ScanId] = @ScanId)
            AND (@ChapterId IS NULL OR cr.[ChapterId] = @ChapterId)
        ORDER BY
            CASE WHEN cr.[Status] = 'PENDING' THEN 0 ELSE 1 END,
            cr.[CreatedAt] DESC;
        RETURN;
    END

    -- =========================================================================
    -- ACTION 2: CHECK My Report for Chapter (Prevenir duplicados antes de enviar)
    -- =========================================================================
    IF @Action = 2
    BEGIN
        SELECT TOP 1
            cr.[ReportId],
            cr.[ChapterId],
            cr.[ChapterNumber],
            cr.[ChapterTitle],
            cr.[MangaId],
            cr.[ScanId],
            cr.[UserId],
            cr.[Reason],
            cr.[Description],
            cr.[Status],
            cr.[AdminNotes],
            cr.[ActionTaken],
            cr.[CreatedAt]
        FROM [dbo].[ChapterReports] cr
        WHERE cr.[ChapterId] = @ChapterId
          AND cr.[UserId] = @UserId;
        RETURN;
    END

    -- =========================================================================
    -- ACTION 3: INSERT Report (1 reporte único por capítulo por persona)
    -- =========================================================================
    IF @Action = 3
    BEGIN
        DECLARE @FoundMangaId INT;
        DECLARE @FoundScanId INT;
        DECLARE @FoundChapterNumber INT;
        DECLARE @FoundChapterTitle VARCHAR(255);

        SELECT
            @FoundMangaId = [MangaId],
            @FoundScanId = [ScanId],
            @FoundChapterNumber = [ChapterNumber],
            @FoundChapterTitle = [ChapterTitle]
        FROM [dbo].[MangaChapters]
        WHERE [ChapterId] = @ChapterId;

        IF @FoundMangaId IS NULL
        BEGIN
            SELECT 404 AS [Status], 'El capítulo indicado no existe o ya fue eliminado.' AS [ErrorDescription];
            RETURN;
        END

        -- Regla de negocio: Solo 1 reporte por capítulo por persona
        IF EXISTS (SELECT 1 FROM [dbo].[ChapterReports] WHERE [ChapterId] = @ChapterId AND [UserId] = @UserId)
        BEGIN
            SELECT 409 AS [Status], 'Ya has reportado este capítulo anteriormente. Solo se permite un reporte por capítulo por usuario.' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            INSERT INTO [dbo].[ChapterReports]
            (
                [ChapterId],
                [ChapterNumber],
                [ChapterTitle],
                [MangaId],
                [ScanId],
                [UserId],
                [Reason],
                [Description],
                [Status],
                [CreatedAt],
                [UpdatedAt]
            )
            VALUES
            (
                @ChapterId,
                @FoundChapterNumber,
                @FoundChapterTitle,
                @FoundMangaId,
                @FoundScanId,
                @UserId,
                UPPER(TRIM(@Reason)),
                NULLIF(TRIM(@Description), ''),
                'PENDING',
                CURRENT_TIMESTAMP,
                CURRENT_TIMESTAMP
            );

            SELECT 200 AS [Status], SCOPE_IDENTITY() AS [NewReportId];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
        RETURN;
    END

    -- =========================================================================
    -- ACTION 4: REVIEW / RESOLVE Report (Auditoría y acciones del Admin)
    -- =========================================================================
    IF @Action = 4
    BEGIN
        DECLARE @TargetChapterId INT;
        SELECT @TargetChapterId = [ChapterId] FROM [dbo].[ChapterReports] WHERE [ReportId] = @ReportId;

        IF NOT EXISTS (SELECT 1 FROM [dbo].[ChapterReports] WHERE [ReportId] = @ReportId)
        BEGIN
            SELECT 404 AS [Status], 'Reporte no encontrado.' AS [ErrorDescription];
            RETURN;
        END

        BEGIN TRY
            DECLARE @FinalStatus VARCHAR(20) = UPPER(ISNULL(NULLIF(TRIM(@Status), ''), 'RESOLVED'));
            DECLARE @FinalAction VARCHAR(100) = UPPER(ISNULL(NULLIF(TRIM(@ActionTaken), ''), 'NONE'));

            -- Si la acción del administrador es eliminar el capítulo defectuoso
            IF @FinalAction = 'DELETE_CHAPTER' AND @TargetChapterId IS NOT NULL
            BEGIN
                -- Resolver todos los reportes pendientes vinculados a este capítulo
                UPDATE [dbo].[ChapterReports]
                SET
                    [Status] = 'RESOLVED',
                    [ActionTaken] = 'CHAPTER_DELETED',
                    [AdminNotes] = ISNULL(NULLIF(TRIM(@AdminNotes), ''), 'Capítulo eliminado por administración tras auditoría.'),
                    [ReviewedBy] = @ReviewedBy,
                    [ReviewedAt] = CURRENT_TIMESTAMP,
                    [UpdatedAt] = CURRENT_TIMESTAMP
                WHERE [ChapterId] = @TargetChapterId
                  AND ([Status] = 'PENDING' OR [ReportId] = @ReportId);

                -- Eliminar páginas físicas de BD y el capítulo (ChapterId en ChapterReports pasa a NULL automáticamente)
                DELETE FROM [dbo].[MangaPages] WHERE [ChapterId] = @TargetChapterId;
                DELETE FROM [dbo].[MangaChapters] WHERE [ChapterId] = @TargetChapterId;
            END
            ELSE
            BEGIN
                -- Si la acción es ocultar el capítulo temporalmente
                IF @FinalAction = 'HIDE_CHAPTER' AND @TargetChapterId IS NOT NULL
                BEGIN
                    UPDATE [dbo].[MangaChapters]
                    SET [ChapterStatus] = 0
                    WHERE [ChapterId] = @TargetChapterId;
                END

                UPDATE [dbo].[ChapterReports]
                SET
                    [Status] = @FinalStatus,
                    [ActionTaken] = @FinalAction,
                    [AdminNotes] = NULLIF(TRIM(@AdminNotes), ''),
                    [ReviewedBy] = @ReviewedBy,
                    [ReviewedAt] = CURRENT_TIMESTAMP,
                    [UpdatedAt] = CURRENT_TIMESTAMP
                WHERE [ReportId] = @ReportId;
            END

            SELECT 200 AS [Status];
        END TRY
        BEGIN CATCH
            SELECT 500 AS [Status], ERROR_MESSAGE() AS [ErrorDescription];
        END CATCH
        RETURN;
    END

    -- =========================================================================
    -- ACTION 5: SELECT Reports & Warnings for Scan Group Owner / Member
    -- (No expone datos personales del usuario que reportó para privacidad)
    -- =========================================================================
    IF @Action = 5
    BEGIN
        SELECT
            cr.[ReportId],
            cr.[ChapterId],
            cr.[ChapterNumber],
            cr.[ChapterTitle],
            cr.[MangaId],
            m.[MangaTittle],
            cr.[ScanId],
            sg.[ScanName],
            cr.[Reason],
            cr.[Description],
            cr.[Status],
            cr.[AdminNotes],
            cr.[ActionTaken],
            rev.[Username] AS [ReviewerUsername],
            cr.[ReviewedAt],
            cr.[CreatedAt],
            CAST(CASE WHEN mc.[ChapterId] IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS [ChapterExists]
        FROM [dbo].[ChapterReports] cr
        INNER JOIN [dbo].[Mangas] m ON cr.[MangaId] = m.[MangaId]
        INNER JOIN [dbo].[ScansGroups] sg ON cr.[ScanId] = sg.[ScanId]
        LEFT JOIN [dbo].[Users] rev ON cr.[ReviewedBy] = rev.[UserId]
        LEFT JOIN [dbo].[MangaChapters] mc ON cr.[ChapterId] = mc.[ChapterId]
        WHERE (
            sg.[CreatedBy] = @UserId
            OR cr.[ScanId] IN (SELECT [ScanId] FROM [dbo].[Users] WHERE [UserId] = @UserId AND [ScanId] IS NOT NULL)
            OR (@ScanId IS NOT NULL AND cr.[ScanId] = @ScanId)
        )
        AND cr.[Status] <> 'DISMISSED'
        ORDER BY
            CASE WHEN cr.[ActionTaken] = 'SCAN_WARNED' THEN 0 WHEN cr.[Status] = 'PENDING' THEN 1 ELSE 2 END,
            cr.[CreatedAt] DESC;
        RETURN;
    END
END
RETURN 0;
