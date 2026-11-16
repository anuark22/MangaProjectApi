using Models;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IScanGroupApplicationsService
    {
        Response<ScanGroupApplication> GetApplications(string? status = null, int? applicationId = null);
        Response<ScanGroupApplication> GetMyApplication(int userId);
        Task<Response<ScanGroupApplication>> Apply(ScanGroupApplyDTO model, int userId);
        Task<Response<DBClass>> ReviewApplication(ReviewScanGroupApplicationDTO model, int adminUserId);
    }
}
