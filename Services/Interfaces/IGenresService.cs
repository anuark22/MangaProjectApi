using System.Threading.Tasks;
using Models;

namespace Services.Interfaces
{
    public interface IGenresService
    {
        Response<Genres> GetGenres(int? genreId = null, string? genreName = null);
        Task<Response<Genres>> CreateGenre(GenreCreateDTO model);
        Task<Response<Genres>> UpdateGenre(GenreUpdateDTO model);
        Task<Response<Genres>> DeleteGenre(int genreId);
    }
}