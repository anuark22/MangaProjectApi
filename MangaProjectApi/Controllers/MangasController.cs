using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MangaProjectApi.CustomMiddleware;
using Models;
using Services.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MangaProjectApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class MangasController : HandlerController
    {
        private readonly IMangasService _mangasService;
        private readonly IWebHostEnvironment _env;

        public MangasController(IMangasService mangasService, IWebHostEnvironment env)
        {
            _mangasService = mangasService;
            _env = env;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetMangas(int? mangaId, string? mangaTittle, int? mangaTypeId)
        {
            Response<Mangas> response = _mangasService.GetMangas(mangaId, mangaTittle, mangaTypeId);
            return ResponseHandler(response);
        }

        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> CreateManga([FromForm] MangaCreateDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Extraer el ID del usuario autenticado desde los claims del token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { Status = 401, ErrorDescription = "Token inválido o usuario no identificado." });
            }

            Response<Mangas> response = await _mangasService.CreateManga(model, userId);
            return ResponseHandler(response);
        }

        [HttpPut]
        [AdminOnly]
        public async Task<IActionResult> UpdateManga([FromForm] MangaUpdateDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<Mangas> response = await _mangasService.UpdateManga(model);
            return ResponseHandler(response);
        }

        [HttpDelete]
        [AdminOnly]
        public async Task<IActionResult> DeleteManga(int mangaId)
        {
            Response<Mangas> response = await _mangasService.DeleteManga(mangaId);
            return ResponseHandler(response);
        }
    }
}