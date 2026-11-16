using Models;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface INoticesService
    {
        Response<Notices> GetNotices(int? noticeId = null, bool? isActive = true);
        Task<Response<Notices>> CreateNotice(NoticeCreateDTO model, int userId);
        Task<Response<Notices>> UpdateNotice(NoticeUpdateDTO model);
        Task<Response<Notices>> DeleteNotice(int noticeId);
    }
}
