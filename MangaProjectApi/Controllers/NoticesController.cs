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
    public class NoticesController : HandlerController
    {
        private readonly INoticesService _noticesService;

        public NoticesController(INoticesService noticesService)
        {
            _noticesService = noticesService;
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
            return ResponseHandler(response);
        }

        [HttpPatch]
        [AdminOnly]
        public async Task<IActionResult> ToggleActive(int noticeId)
        {
            Response<Notices> response = await _noticesService.ToggleNoticeActive(noticeId);
            return ResponseHandler(response);
        }

        [HttpPatch]
        [AdminOnly]
        public async Task<IActionResult> ToggleFeatured(int noticeId)
        {
            Response<Notices> response = await _noticesService.ToggleNoticeFeatured(noticeId);
            return ResponseHandler(response);
        }

        [HttpDelete]
        [AdminOnly]
        public async Task<IActionResult> DeleteNotice(int noticeId)
        {
            Response<Notices> response = await _noticesService.DeleteNotice(noticeId);
            return ResponseHandler(response);
        }
    }
}
