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
    public class UserMangaListsController : HandlerController
    {
        private readonly IUserMangaListsService _userMangaListsService;

        public UserMangaListsController(IUserMangaListsService userMangaListsService)
        {
            _userMangaListsService = userMangaListsService;
        }

        [HttpGet]
        public IActionResult GetMyList(string? readingStatus)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { Status = 401, ErrorDescription = "Token inválido o usuario no identificado." });
            }

            Response<UserMangaLists> response = _userMangaListsService.GetUserList(userId, readingStatus);
            return ResponseHandler(response);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdateManga([FromBody] AddUserMangaDTO model)
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

            Response<UserMangaLists> response = await _userMangaListsService.AddOrUpdateMangaInList(model, userId);
            return ResponseHandler(response);
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveManga(int mangaId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { Status = 401, ErrorDescription = "Token inválido o usuario no identificado." });
            }

            Response<UserMangaLists> response = await _userMangaListsService.RemoveMangaFromList(mangaId, userId);
            return ResponseHandler(response);
        }

        [HttpGet]
        public IActionResult CheckMangaInList(int mangaId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { Status = 401, ErrorDescription = "Token inválido o usuario no identificado." });
            }

            Response<UserMangaStatusCheckDTO> response = _userMangaListsService.CheckMangaInList(mangaId, userId);
            return ResponseHandler(response);
        }
    }
}
