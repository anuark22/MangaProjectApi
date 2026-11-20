using MangaProjectApi.CustomMiddleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services.Interfaces;

namespace MangaProjectApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class AuditLogsController : HandlerController
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet]
        [AdminOnly]
        public IActionResult GetAuditLogs(
            string? entityType = null,
            string? action = null,
            int? userId = null,
            string? search = null,
            int top = 150)
        {
            var userInfo = HttpContext.Items["Client"] as UsersDTO;
            if (userInfo?.RoleId != (int)RolesEnum.Admin)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    Status = 403,
                    Error = true,
                    ErrorDescription = "Solo los administradores pueden consultar la auditoría del sistema."
                });
            }

            Response<AuditLogDTO> response = _auditLogService.GetAuditLogs(entityType, action, userId, search, top);
            return ResponseHandler(response);
        }
    }
}
