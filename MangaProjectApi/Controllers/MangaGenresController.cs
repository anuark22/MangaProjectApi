using Microsoft.AspNetCore.Mvc;
using MangaProjectApi.CustomMiddleware;
using Models;
using Services.Interfaces;
using System.Threading.Tasks;

namespace MangaProjectApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class MangaGenresController : HandlerController
    {
        private readonly IMangaGenresService _mangaGenresService;

        public MangaGenresController(IMangaGenresService mangaGenresService)
        {
            _mangaGenresService = mangaGenresService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetMangaGenres(int? genreId, int? mangaId)
        {
            Response<MangaGenres> response = _mangaGenresService.GetMangaGenres(genreId, mangaId);
            return ResponseHandler(response);
        }

        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> CreateMangaGenre([FromBody] MangaGenreCreateDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<MangaGenres> response = await _mangaGenresService.CreateMangaGenre(model);
            return ResponseHandler(response);
        }

        [HttpDelete]
        [AdminOnly]
        public async Task<IActionResult> DeleteMangaGenre(int genreId, int mangaId)
        {
            Response<MangaGenres> response = await _mangaGenresService.DeleteMangaGenre(genreId, mangaId);
            return ResponseHandler(response);
        }
    }
}