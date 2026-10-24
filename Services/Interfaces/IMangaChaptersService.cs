using Models;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IMangaChaptersService
    {
        Response<MangaChapters> GetMangaChapters(int? chapterId = null, int? mangaId = null);
        Task<Response<MangaChapters>> CreateMangaChapter(MangaChapterCreateDTO model, int userId);
        Task<Response<MangaChapters>> UpdateMangaChapter(MangaChapterUpdateDTO model);
        Task<Response<MangaChapters>> DeleteMangaChapter(int chapterId);
    }
}