using Models;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IUserMangaListsService
    {
        Response<UserMangaLists> GetUserList(int userId, string? readingStatus = null);
        Task<Response<UserMangaLists>> AddOrUpdateMangaInList(AddUserMangaDTO model, int userId);
        Task<Response<UserMangaLists>> RemoveMangaFromList(int mangaId, int userId);
        Response<UserMangaStatusCheckDTO> CheckMangaInList(int mangaId, int userId);
        Response<UserReadChapterDTO> GetReadChapters(int userId, int? mangaId = null);
        Task<Response<UserReadChapterDTO>> MarkChapterAsRead(int userId, int mangaId, int chapterId);
        Task<Response<DBClass>> UnmarkChapterAsRead(int userId, int chapterId);
        Response<UserMangaProgressDTO> GetMyReadingProgress(int userId);
    }
}
