using Models;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IMangaGenresService
    {
        Response<MangaGenres> GetMangaGenres(int? genreId = null, int? mangaId = null);
        Task<Response<MangaGenres>> CreateMangaGenre(MangaGenreCreateDTO model);
        Task<Response<MangaGenres>> DeleteMangaGenre(int genreId, int mangaId);
    }
}