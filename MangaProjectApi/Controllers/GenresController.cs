using Microsoft.AspNetCore.Mvc;
using Models;
using Services.Interfaces;
using MangaProjectApi.CustomMiddleware;
using System.Linq;
using System.Threading.Tasks;

namespace MangaProjectApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class GenresController : HandlerController
    {
        private readonly IGenresService _genresService;
        private readonly IAuditLogService _auditLogService;
        private readonly IWebHostEnvironment _env;

        public GenresController(IGenresService genresService, IAuditLogService auditLogService, IWebHostEnvironment env)
        {
            _genresService = genresService;
            _auditLogService = auditLogService;
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
            if (!response.Error && response.Status == 200)
            {
                var created = response.Items?.FirstOrDefault();
                _auditLogService.Log(HttpContext, "CREATE", "Genre", created?.GenreId, created?.GenreName ?? model.GenreName, "Nuevo género creado");
            }
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
            if (!response.Error && response.Status == 200)
            {
                _auditLogService.Log(HttpContext, "UPDATE", "Genre", model.GenreId, model.GenreName, "Género actualizado");
            }
            return ResponseHandler(response);
        }

        // =========================================================================
        // DELETE: Genres/DeleteGenre (PROTEGIDO - Requiere Token / Admin)
        // =========================================================================
        [HttpDelete]
        [AdminOnly] // Opcional: Quítalo si cualquier usuario autenticado puede borrar
        public async Task<IActionResult> DeleteGenre(int genreId)
        {
            var existing = _genresService.GetGenres(genreId, null)?.Items?.FirstOrDefault();
            Response<Genres> response = await _genresService.DeleteGenre(genreId);
            if (!response.Error && response.Status == 200)
            {
                _auditLogService.Log(HttpContext, "DELETE", "Genre", genreId, existing?.GenreName ?? $"Genre #{genreId}", "Género eliminado");
            }
            return ResponseHandler(response);
        }
    }
}