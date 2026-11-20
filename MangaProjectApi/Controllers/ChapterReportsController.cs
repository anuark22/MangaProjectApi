using MangaProjectApi.CustomMiddleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services.Interfaces;
using System.Security.Claims;

namespace MangaProjectApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ChapterReportsController : HandlerController
    {
        private readonly IChapterReportsService _service;
        private readonly IAntiSpamService _antiSpamService;
        private readonly IAuditLogService _auditLogService;

        public ChapterReportsController(IChapterReportsService service, IAntiSpamService antiSpamService, IAuditLogService auditLogService)
        {
            _service = service;
            _antiSpamService = antiSpamService;
            _auditLogService = auditLogService;
        }

        /// <summary>
        /// (Solo Administradores) Obtiene los reportes de capítulos con filtros opcionales por estado, manga o scan.
        /// </summary>
        [HttpGet]
        [AdminOnly]
        public IActionResult GetReports(string? status = null, int? reportId = null, int? mangaId = null, int? scanId = null)
        {
            Response<ChapterReport> response = _service.GetReports(status, reportId, mangaId, scanId);
            return ResponseHandler(response);
        }

        /// <summary>
        /// Permite al Scan Group autenticado consultar los reportes y advertencias de auditoría sobre sus capítulos.
        /// </summary>
        [HttpGet]
        public IActionResult GetMyScanReports(int? scanId = null)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { Status = 401, ErrorDescription = "Token inválido o usuario no identificado." });
            }

            Response<ChapterReport> response = _service.GetMyScanReports(userId, scanId);
            return ResponseHandler(response);
        }

        /// <summary>
        /// Permite al usuario autenticado verificar si ya reportó un capítulo específico antes de abrir/enviar un nuevo reporte.
        /// </summary>
        [HttpGet]
        public IActionResult GetMyReportForChapter(int chapterId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { Status = 401, ErrorDescription = "Token inválido o usuario no identificado." });
            }

            Response<ChapterReport> response = _service.GetMyReportForChapter(chapterId, userId);
            return ResponseHandler(response);
        }

        /// <summary>
        /// Crea un reporte de capítulo. Limitado a 1 reporte por capítulo por persona y protegido con anti-spam.
        /// </summary>
        [HttpPost]
        public IActionResult CreateReport([FromBody] CreateChapterReportDTO model)
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

            // Detección de bot trampa honeypot
            if (_antiSpamService.IsHoneypotTriggered(model.Honeypot))
            {
                return BadRequest(new { Status = 400, Error = true, ErrorDescription = "Solicitud rechazada." });
            }

            // Flood control individual: máximo 1 reporte cada 5 segundos por usuario
            var (isRateLimited, secondsRemaining) = _antiSpamService.CheckActionCooldown("chapter_report", userId.ToString(), 5);
            if (isRateLimited)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, new
                {
                    Status = 429,
                    Error = true,
                    SecondsRemaining = secondsRemaining,
                    ErrorDescription = $"Por favor espera {secondsRemaining} segundos antes de enviar otro reporte."
                });
            }

            Response<ChapterReport> response = _service.CreateReport(model, userId);
            if (response.Status == 200 && !response.Error)
            {
                _antiSpamService.RecordAction("chapter_report", userId.ToString());
                _auditLogService.Log(HttpContext, "CREATE", "ChapterReport", model.ChapterId, $"Reporte Cap. #{model.ChapterId}", $"Motivo: {model.Reason}");
            }

            return ResponseHandler(response);
        }

        /// <summary>
        /// (Solo Administradores) Audita y resuelve/descarta un reporte, con opción de eliminar u ocultar el capítulo.
        /// </summary>
        [HttpPost]
        [AdminOnly]
        public IActionResult ReviewReport([FromBody] ReviewChapterReportDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int adminUserId))
            {
                return Unauthorized(new { Status = 401, ErrorDescription = "Token inválido o administrador no identificado." });
            }

            Response<ChapterReport> response = _service.ReviewReport(model, adminUserId);
            if (!response.Error && response.Status == 200)
            {
                _auditLogService.Log(HttpContext, "RESOLVE", "ChapterReport", model.ReportId, $"Reporte #{model.ReportId}", $"Estado: {model.Status} | Acción: {model.ActionTaken} | Notas: {model.AdminNotes}");
            }
            return ResponseHandler(response);
        }
    }
}
