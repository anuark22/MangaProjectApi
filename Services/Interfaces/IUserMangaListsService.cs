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
    }
}
