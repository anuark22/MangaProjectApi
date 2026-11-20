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
    public class ScanGroupApplicationsController : HandlerController
    {
        private readonly IScanGroupApplicationsService _service;
        private readonly IAuditLogService _auditLogService;

        public ScanGroupApplicationsController(IScanGroupApplicationsService service, IAuditLogService auditLogService)
        {
            _service = service;
            _auditLogService = auditLogService;
        }

        /// <summary>
        /// Permite a un usuario autenticado enviar una solicitud para fundar o registrar un Scan Group.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Apply([FromForm] ScanGroupApplyDTO model)
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

            Response<ScanGroupApplication> response = await _service.Apply(model, userId);
            if (!response.Error && response.Status == 200)
            {
                var created = response.Items?.FirstOrDefault();
                _auditLogService.Log(HttpContext, "CREATE", "ScanApplication", created?.ApplicationId, model.ScanName, "Usuario envió solicitud para crear Grupo Scan");
            }
            return ResponseHandler(response);
        }

        /// <summary>
        /// Permite al usuario autenticado consultar el estado de su solicitud actual (PENDING, APPROVED, REJECTED).
        /// </summary>
        [HttpGet]
        public IActionResult MyApplication()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { Status = 401, ErrorDescription = "Token inválido o usuario no identificado." });
            }

            Response<ScanGroupApplication> response = _service.GetMyApplication(userId);
            return ResponseHandler(response);
        }

        /// <summary>
        /// (Solo Administradores) Consulta el listado de solicitudes, con filtro opcional por estado ('PENDING', 'APPROVED', 'REJECTED') o ID.
        /// </summary>
        [HttpGet]
        [AdminOnly]
        public IActionResult GetApplications([FromQuery] string? status, [FromQuery] int? applicationId)
        {
            Response<ScanGroupApplication> response = _service.GetApplications(status, applicationId);
            return ResponseHandler(response);
        }

        /// <summary>
        /// (Solo Administradores) Aprueba o rechaza una solicitud. Al aprobar, crea automáticamente el ScanGroup y actualiza el rol del usuario a Scan (3).
        /// </summary>
        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> ReviewApplication([FromBody] ReviewScanGroupApplicationDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            int adminUserId = userIdClaim != null && int.TryParse(userIdClaim.Value, out int id) ? id : 0;

            Response<DBClass> response = await _service.ReviewApplication(model, adminUserId);
            if (!response.Error && response.Status == 200)
            {
                string auditAction = model.IsApproved ? "APPROVE" : "REJECT";
                _auditLogService.Log(HttpContext, auditAction, "ScanApplication", model.ApplicationId, $"Solicitud #{model.ApplicationId}", model.AdminNotes ?? (model.IsApproved ? "Solicitud aprobada" : "Solicitud rechazada"));
            }
            return ResponseHandler(response);
        }
    }
}
