using Models;
using Services.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Services
{
    public class ChapterReportsService : IChapterReportsService
    {
        private readonly ICommonService _commonService;

        public ChapterReportsService(ICommonService commonService)
        {
            _commonService = commonService;
        }

        public Response<ChapterReport> GetReports(string? status = null, int? reportId = null, int? mangaId = null, int? scanId = null)
        {
            Response<ChapterReport> response = new();
            object param = new
            {
                Action = 1,
                Status = string.IsNullOrWhiteSpace(status) || status.Equals("ALL", System.StringComparison.OrdinalIgnoreCase) ? null : status.Trim().ToUpper(),
                ReportId = reportId,
                MangaId = mangaId,
                ScanId = scanId
            };

            response.Items = _commonService.Query<ChapterReport>("ChapterReportsProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);
            return response;
        }

        public Response<ChapterReport> GetMyReportForChapter(int chapterId, int userId)
        {
            Response<ChapterReport> response = new();
            object param = new
            {
                Action = 2,
                ChapterId = chapterId,
                UserId = userId
            };

            response.Items = _commonService.Query<ChapterReport>("ChapterReportsProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);
            return response;
        }

        public Response<ChapterReport> CreateReport(CreateChapterReportDTO model, int userId)
        {
            Response<ChapterReport> response = new();
            object param = new
            {
                Action = 3,
                ChapterId = model.ChapterId,
                UserId = userId,
                Reason = model.Reason,
                Description = model.Description
            };

            IEnumerable<DBClass> dbResponse = _commonService.Query<DBClass>("ChapterReportsProcedures", param, CommandType.StoredProcedure);
            var firstResult = dbResponse.FirstOrDefault();

            if (firstResult != null)
            {
                response.ParseExecution(firstResult.Status, firstResult.ErrorDescription);
            }
            else
            {
                response.ParseExecution(500, "Error inesperado al registrar el reporte del capítulo.");
            }

            return response;
        }

        public Response<ChapterReport> ReviewReport(ReviewChapterReportDTO model, int adminUserId)
        {
            Response<ChapterReport> response = new();
            object param = new
            {
                Action = 4,
                ReportId = model.ReportId,
                Status = model.Status,
                ActionTaken = model.ActionTaken,
                AdminNotes = model.AdminNotes,
                ReviewedBy = adminUserId
            };

            IEnumerable<DBClass> dbResponse = _commonService.Query<DBClass>("ChapterReportsProcedures", param, CommandType.StoredProcedure);
            var firstResult = dbResponse.FirstOrDefault();

            if (firstResult != null)
            {
                response.ParseExecution(firstResult.Status, firstResult.ErrorDescription);
            }
            else
            {
                response.ParseExecution(500, "Error inesperado al procesar la auditoría del reporte.");
            }

            return response;
        }
    }
}
