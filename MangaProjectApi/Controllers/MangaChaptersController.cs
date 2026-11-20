using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MangaProjectApi.CustomMiddleware;
using Models;
using Services.Interfaces;
using System.Data;
using System.Linq;
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
        private readonly ICommonService _commonService;
        private readonly IAuditLogService _auditLogService;
        private readonly IWebHostEnvironment _env;

        public MangaChaptersController(
            IMangaChaptersService mangaChaptersService,
            ICommonService commonService,
            IAuditLogService auditLogService,
            IWebHostEnvironment env)
        {
            _mangaChaptersService = mangaChaptersService;
            _commonService = commonService;
            _auditLogService = auditLogService;
            _env = env;
        }

        private int? ResolveUserScanId(int userId)
        {
            const string sql = @"
                SELECT TOP 1 COALESCE(u.ScanId, sg.ScanId)
                FROM [dbo].[Users] u
                LEFT JOIN [dbo].[ScansGroups] sg ON sg.[CreatedBy] = u.[UserId]
                WHERE u.[UserId] = @UserId";
            return _commonService.Query<int?>(sql, new { UserId = userId }, CommandType.Text).FirstOrDefault();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetMangaChapters(int? chapterId, int? mangaId)
        {
            Response<MangaChapters> response = _mangaChaptersService.GetMangaChapters(chapterId, mangaId);
            return ResponseHandler(response);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetChapterPages(int chapterId)
        {
            Response<MangaPages> response = await _mangaChaptersService.GetChapterPages(chapterId);
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

            var userInfo = HttpContext.Items["Client"] as UsersDTO;
            bool isAdmin = userInfo?.RoleId == (int)RolesEnum.Admin;

            // Los usuarios Scan solo pueden subir capítulos bajo su propio Grupo Scan asignado
            if (!isAdmin)
            {
                int? myScanId = ResolveUserScanId(userId);
                if (!myScanId.HasValue || myScanId.Value <= 0)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new
                    {
                        Status = 403,
                        Error = true,
                        ErrorDescription = "No tienes un Grupo Scan asignado para publicar capítulos."
                    });
                }

                if (model.ScanId > 0 && model.ScanId != myScanId.Value)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new
                    {
                        Status = 403,
                        Error = true,
                        ErrorDescription = "Solo puedes subir capítulos a nombre de tu propio Grupo Scan."
                    });
                }

                model.ScanId = myScanId.Value;
            }

            Response<MangaChapters> response = await _mangaChaptersService.CreateMangaChapter(model, userId);
            if (!response.Error && response.Status == 200)
            {
                var created = response.Items?.FirstOrDefault();
                _auditLogService.Log(
                    HttpContext,
                    "CREATE",
                    "Chapter",
                    created?.ChapterId,
                    $"Cap. {model.ChapterNumber} - {model.ChapterTitle}",
                    $"MangaId: {model.MangaId}, Páginas: {model.Files?.Count ?? 0}",
                    model.ScanId);
            }
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

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            var userInfo = HttpContext.Items["Client"] as UsersDTO;
            bool isAdmin = userInfo?.RoleId == (int)RolesEnum.Admin;

            var existing = _mangaChaptersService.GetMangaChapters(model.ChapterId, null)?.Items?.FirstOrDefault();
            if (!isAdmin && userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                int? myScanId = ResolveUserScanId(userId);
                if (existing != null && (!myScanId.HasValue || existing.ScanId != myScanId.Value))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new
                    {
                        Status = 403,
                        Error = true,
                        ErrorDescription = "Solo puedes editar capítulos pertenecientes a tu propio Grupo Scan."
                    });
                }
            }

            Response<MangaChapters> response = await _mangaChaptersService.UpdateMangaChapter(model);
            if (!response.Error && response.Status == 200)
            {
                _auditLogService.Log(
                    HttpContext,
                    "UPDATE",
                    "Chapter",
                    model.ChapterId,
                    $"Cap. {model.ChapterNumber ?? existing?.ChapterNumber} - {model.ChapterTitle ?? existing?.ChapterTitle}",
                    $"MangaId: {existing?.MangaId}",
                    existing?.ScanId);
            }
            return ResponseHandler(response);
        }

        [HttpDelete]
        [AdminOnly]
        public async Task<IActionResult> DeleteMangaChapter(int chapterId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            var userInfo = HttpContext.Items["Client"] as UsersDTO;
            bool isAdmin = userInfo?.RoleId == (int)RolesEnum.Admin;

            var existing = _mangaChaptersService.GetMangaChapters(chapterId, null)?.Items?.FirstOrDefault();
            if (!isAdmin && userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                int? myScanId = ResolveUserScanId(userId);
                if (existing != null && (!myScanId.HasValue || existing.ScanId != myScanId.Value))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new
                    {
                        Status = 403,
                        Error = true,
                        ErrorDescription = "Solo puedes eliminar capítulos pertenecientes a tu propio Grupo Scan."
                    });
                }
            }

            Response<MangaChapters> response = await _mangaChaptersService.DeleteMangaChapter(chapterId);
            if (!response.Error && response.Status == 200)
            {
                _auditLogService.Log(
                    HttpContext,
                    "DELETE",
                    "Chapter",
                    chapterId,
                    existing != null ? $"Cap. {existing.ChapterNumber} - {existing.ChapterTitle}" : $"Capítulo #{chapterId}",
                    $"MangaId: {existing?.MangaId}",
                    existing?.ScanId);
            }
            return ResponseHandler(response);
        }
    }
}