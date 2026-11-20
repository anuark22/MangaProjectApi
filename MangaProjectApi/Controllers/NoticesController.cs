using Microsoft.AspNetCore.Mvc;
using MangaProjectApi.CustomMiddleware;
using Models;
using Services.Interfaces;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MangaProjectApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class NoticesController : HandlerController
    {
        private readonly INoticesService _noticesService;
        private readonly IAuditLogService _auditLogService;

        public NoticesController(INoticesService noticesService, IAuditLogService auditLogService)
        {
            _noticesService = noticesService;
            _auditLogService = auditLogService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetNotices(int? noticeId, bool? isActive = true, bool includeInactive = false)
        {
            bool? filterActive = includeInactive ? null : isActive;
            Response<Notices> response = _noticesService.GetNotices(noticeId, filterActive);
            return ResponseHandler(response);
        }

        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> CreateNotice([FromForm] NoticeCreateDTO model)
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

            Response<Notices> response = await _noticesService.CreateNotice(model, userId);
            if (!response.Error && response.Status == 200)
            {
                var created = response.Items?.FirstOrDefault();
                _auditLogService.Log(HttpContext, "CREATE", "Notice", created?.NoticeId, model.Title, $"Categoría: {model.Category}");
            }
            return ResponseHandler(response);
        }

        [HttpPut]
        [AdminOnly]
        public async Task<IActionResult> UpdateNotice([FromForm] NoticeUpdateDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<Notices> response = await _noticesService.UpdateNotice(model);
            if (!response.Error && response.Status == 200)
            {
                _auditLogService.Log(HttpContext, "UPDATE", "Notice", model.NoticeId, model.Title, "Noticia actualizada");
            }
            return ResponseHandler(response);
        }

        [HttpPatch]
        [AdminOnly]
        public async Task<IActionResult> ToggleActive(int noticeId)
        {
            Response<Notices> response = await _noticesService.ToggleNoticeActive(noticeId);
            if (!response.Error && response.Status == 200)
            {
                _auditLogService.Log(HttpContext, "UPDATE", "Notice", noticeId, $"Noticia #{noticeId}", "Estado activo/inactivo alternado");
            }
            return ResponseHandler(response);
        }

        [HttpPatch]
        [AdminOnly]
        public async Task<IActionResult> ToggleFeatured(int noticeId)
        {
            Response<Notices> response = await _noticesService.ToggleNoticeFeatured(noticeId);
            if (!response.Error && response.Status == 200)
            {
                _auditLogService.Log(HttpContext, "UPDATE", "Notice", noticeId, $"Noticia #{noticeId}", "Estado destacado alternado");
            }
            return ResponseHandler(response);
        }

        [HttpDelete]
        [AdminOnly]
        public async Task<IActionResult> DeleteNotice(int noticeId)
        {
            Response<Notices> response = await _noticesService.DeleteNotice(noticeId);
            if (!response.Error && response.Status == 200)
            {
                _auditLogService.Log(HttpContext, "DELETE", "Notice", noticeId, $"Noticia #{noticeId}", "Noticia eliminada");
            }
            return ResponseHandler(response);
        }
    }
}
