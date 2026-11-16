using Models;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IMangasService
    {
        Response<Mangas> GetMangas(
            int? mangaId = null, 
            string? mangaTittle = null, 
            int? mangaTypeId = null,
            int? demographyId = null,
            string? status = null,
            bool? isAmateur = null,
            bool? isErotic = null,
            bool? isFeatured = null
        );
        Response<Mangas> GetPopularMangas(int top = 10);
        Response<MangaDetailDTO> GetMangaDetail(int mangaId);
        Task<Response<Mangas>> CreateManga(MangaCreateDTO model, int userId);
        Task<Response<Mangas>> UpdateManga(MangaUpdateDTO model);
        Task<Response<Mangas>> DeleteManga(int mangaId);
    }
}
