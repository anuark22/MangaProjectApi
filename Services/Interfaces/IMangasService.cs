using Models;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IMangasService
    {
        Response<Mangas> GetMangas(int? mangaId = null, string? mangaTittle = null, int? mangaTypeId = null);
        Task<Response<Mangas>> CreateManga(MangaCreateDTO model, int userId);
        Task<Response<Mangas>> UpdateManga(MangaUpdateDTO model);
        Task<Response<Mangas>> DeleteManga(int mangaId);
    }
}