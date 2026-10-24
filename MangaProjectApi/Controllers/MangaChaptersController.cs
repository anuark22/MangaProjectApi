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
    public class MangaChaptersController : HandlerController
    {
        private readonly IMangaChaptersService _mangaChaptersService;
        private readonly IWebHostEnvironment _env;

        public MangaChaptersController(IMangaChaptersService mangaChaptersService, IWebHostEnvironment env)
        {
            _mangaChaptersService = mangaChaptersService;
            _env = env;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetMangaChapters(int? chapterId, int? mangaId)
        {
            Response<MangaChapters> response = _mangaChaptersService.GetMangaChapters(chapterId, mangaId);
            return ResponseHandler(response);
        }

        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> CreateMangaChapter([FromForm] MangaChapterCreateDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { Status = 401, ErrorDescription = "Token inválido o usuario no identificado." });
            }

            Response<MangaChapters> response = await _mangaChaptersService.CreateMangaChapter(model, userId);
            return ResponseHandler(response);
        }

        [HttpPut]
        [AdminOnly]
        public async Task<IActionResult> UpdateMangaChapter([FromForm] MangaChapterUpdateDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<MangaChapters> response = await _mangaChaptersService.UpdateMangaChapter(model);
            return ResponseHandler(response);
        }

        [HttpDelete]
        [AdminOnly]
        public async Task<IActionResult> DeleteMangaChapter(int chapterId)
        {
            Response<MangaChapters> response = await _mangaChaptersService.DeleteMangaChapter(chapterId);
            return ResponseHandler(response);
        }
    }
}