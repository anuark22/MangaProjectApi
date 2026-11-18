using Models;

namespace Services.Interfaces
{
    public interface IChapterReportsService
    {
        Response<ChapterReport> GetReports(string? status = null, int? reportId = null, int? mangaId = null, int? scanId = null);
        Response<ChapterReport> GetMyScanReports(int userId, int? scanId = null);
        Response<ChapterReport> GetMyReportForChapter(int chapterId, int userId);
        Response<ChapterReport> CreateReport(CreateChapterReportDTO model, int userId);
        Response<ChapterReport> ReviewReport(ReviewChapterReportDTO model, int adminUserId);
    }
}
