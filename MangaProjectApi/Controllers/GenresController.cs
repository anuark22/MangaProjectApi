using Microsoft.AspNetCore.Mvc;
using Models;
using Services.Interfaces;
using MangaProjectApi.CustomMiddleware;
using System.Threading.Tasks;

namespace MangaProjectApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class GenresController : HandlerController
    {
        private readonly IGenresService _genresService;
        private readonly IWebHostEnvironment _env;

        public GenresController(IGenresService genresService, IWebHostEnvironment env)
        {
            _genresService = genresService;
            _env = env;
        }

        // =========================================================================
        // GET: Genres/GetGenres (PÚBLICO - No requiere Token)
        // =========================================================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetGenres(int? genreId, string? genreName)
        {
            Response<Genres> response = _genresService.GetGenres(genreId, genreName);
            return ResponseHandler(response);
        }

        // =========================================================================
        // POST: Genres/CreateGenre (Controlador ultraligero)
        // =========================================================================
        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> CreateGenre([FromForm] GenreCreateDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // El servicio se encarga de todo el trabajo sucio
            Response<Genres> response = await _genresService.CreateGenre(model);
            return ResponseHandler(response);
        }

        // =========================================================================
        // PUT: Genres/UpdateGenre (Recibe [FromForm] para soportar cambio de imagen opcional)
        // =========================================================================
        [HttpPut]
        [AdminOnly]
        public async Task<IActionResult> UpdateGenre([FromForm] GenreUpdateDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Delegamos toda la lógica al servicio
            Response<Genres> response = await _genresService.UpdateGenre(model);
            return ResponseHandler(response);
        }

        // =========================================================================
        // DELETE: Genres/DeleteGenre (PROTEGIDO - Requiere Token / Admin)
        // =========================================================================
        [HttpDelete]
        [AdminOnly] // Opcional: Quítalo si cualquier usuario autenticado puede borrar
        public async Task<IActionResult> DeleteGenre(int genreId)
        {
            Response<Genres> response = await _genresService.DeleteGenre(genreId);
            return ResponseHandler(response);
        }
    }
}