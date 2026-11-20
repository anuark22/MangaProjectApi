using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MangaProjectApi.CustomMiddleware;
using Models;
using Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IAuditLogService _auditLogService;
        private readonly IWebHostEnvironment _env;

        public MangasController(IMangasService mangasService, IAuditLogService auditLogService, IWebHostEnvironment env)
        {
            _mangasService = mangasService;
            _auditLogService = auditLogService;
            _env = env;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetMangas(
            int? mangaId, 
            string? mangaTittle, 
            int? mangaTypeId,
            int? demographyId,
            string? status,
            bool? isAmateur,
            bool? isErotic,
            bool? isFeatured)
        {
            Response<Mangas> response = _mangasService.GetMangas(
                mangaId, 
                mangaTittle, 
                mangaTypeId, 
                demographyId, 
                status, 
                isAmateur, 
                isErotic, 
                isFeatured
            );
            return ResponseHandler(response);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetPopularMangas(int top = 10)
        {
            Response<Mangas> response = _mangasService.GetPopularMangas(top);
            return ResponseHandler(response);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetMangaDetail(int mangaId)
        {
            Response<MangaDetailDTO> response = _mangasService.GetMangaDetail(mangaId);
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

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { Status = 401, ErrorDescription = "Token inválido o usuario no identificado." });
            }

            Response<Mangas> response = await _mangasService.CreateManga(model, userId);
            if (!response.Error && response.Status == 200)
            {
                var created = response.Items?.FirstOrDefault();
                _auditLogService.Log(
                    HttpContext,
                    "CREATE",
                    "Manga",
                    created?.MangaId,
                    created?.MangaTittle ?? model.MangaTittle,
                    $"Estado: {model.Status}, Autor: {model.Mangaka}");
            }
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

            var existing = _mangasService.GetMangas(model.MangaId, null, null, null, null, null, null, null)?.Items?.FirstOrDefault();
            Response<Mangas> response = await _mangasService.UpdateManga(model);

            if (!response.Error && response.Status == 200)
            {
                var changes = new List<string>();
                if (existing != null)
                {
                    if (!string.IsNullOrWhiteSpace(model.MangaTittle) && model.MangaTittle != existing.MangaTittle)
                        changes.Add($"Título: '{existing.MangaTittle}' -> '{model.MangaTittle}'");
                    if (!string.IsNullOrWhiteSpace(model.Status) && model.Status != existing.Status)
                        changes.Add($"Estado: '{existing.Status}' -> '{model.Status}'");
                    if (!string.IsNullOrWhiteSpace(model.Mangaka) && model.Mangaka != existing.Mangaka)
                        changes.Add($"Autor: '{existing.Mangaka}' -> '{model.Mangaka}'");
                    if (model.Rating.HasValue && model.Rating.Value != existing.Rating)
                        changes.Add($"Rating: {existing.Rating} -> {model.Rating.Value}");
                    if (!string.IsNullOrWhiteSpace(model.MangaDescription) && model.MangaDescription != existing.MangaDescription)
                        changes.Add("Sinopsis actualizada");
                }

                string detailText = changes.Count > 0 ? string.Join(" | ", changes) : "Datos del manga actualizados";
                _auditLogService.Log(
                    HttpContext,
                    "UPDATE",
                    "Manga",
                    model.MangaId,
                    model.MangaTittle ?? existing?.MangaTittle,
                    detailText);
            }

            return ResponseHandler(response);
        }

        [HttpDelete]
        [AdminOnly]
        public async Task<IActionResult> DeleteManga(int mangaId)
        {
            var userInfo = HttpContext.Items["Client"] as UsersDTO;
            if (userInfo?.RoleId != (int)RolesEnum.Admin)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    Status = 403,
                    Error = true,
                    ErrorDescription = "Solo los administradores pueden eliminar mangas."
                });
            }

            var existing = _mangasService.GetMangas(mangaId, null, null, null, null, null, null, null)?.Items?.FirstOrDefault();
            Response<Mangas> response = await _mangasService.DeleteManga(mangaId);
            if (!response.Error && response.Status == 200)
            {
                _auditLogService.Log(
                    HttpContext,
                    "DELETE",
                    "Manga",
                    mangaId,
                    existing?.MangaTittle,
                    "Manga eliminado del catálogo");
            }
            return ResponseHandler(response);
        }
    }
}
