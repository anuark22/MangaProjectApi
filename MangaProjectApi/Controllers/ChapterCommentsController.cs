using MangaProjectApi.CustomMiddleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MangaProjectApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ChapterCommentsController : HandlerController
    {
        private readonly IChapterCommentsService _service;
        private readonly IAntiSpamService _antiSpamService;

        public ChapterCommentsController(IChapterCommentsService service, IAntiSpamService antiSpamService)
        {
            _service = service;
            _antiSpamService = antiSpamService;
        }

        /// <summary>
        /// Obtiene todos los comentarios y respuestas en cascada de un capítulo.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetComments(int chapterId)
        {
            Response<ChapterComment> response = _service.GetComments(chapterId);
            return ResponseHandler(response);
        }

        /// <summary>
        /// Publica un comentario o respuesta (soporta texto, imagen subida o URL de GIF).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddComment([FromForm] CreateChapterCommentDTO model)
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

            if (_antiSpamService.IsHoneypotTriggered(model.Honeypot))
            {
                return BadRequest(new { Status = 400, Error = true, ErrorDescription = "Solicitud rechazada." });
            }

            // Flood control individual: 1 comentario cada 4 segundos por usuario
            var (isRateLimited, secondsRemaining) = _antiSpamService.CheckActionCooldown("chapter_comment", userId.ToString(), 4);
            if (isRateLimited)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, new
                {
                    Status = 429,
                    Error = true,
                    SecondsRemaining = secondsRemaining,
                    ErrorDescription = $"Por favor espera {secondsRemaining} segundos antes de comentar nuevamente."
                });
            }

            if (!string.IsNullOrWhiteSpace(model.Content) &&
                _antiSpamService.IsDuplicateContent($"ch_comment_{userId}_{model.ChapterId}", model.Content, 30))
            {
                return BadRequest(new
                {
                    Status = 400,
                    Error = true,
                    ErrorDescription = "Ya publicaste exactamente ese mismo comentario recientemente."
                });
            }

            Response<ChapterComment> response = await _service.AddComment(model, userId);
            if (response.Status == 200 && !response.Error)
            {
                _antiSpamService.RecordAction("chapter_comment", userId.ToString());
            }

            return ResponseHandler(response);
        }

        /// <summary>
        /// Elimina un comentario (Autor o Admin). Si tiene respuestas hijas, preserva el árbol mostrando [Comentario eliminado].
        /// </summary>
        [HttpDelete]
        public IActionResult DeleteComment(int commentId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { Status = 401, ErrorDescription = "Token inválido o usuario no identificado." });
            }

            var roleClaim = User.FindFirst(ClaimTypes.Role);
            bool isAdmin = roleClaim?.Value == "ADMIN" || roleClaim?.Value == "2";

            Response<ChapterComment> response = _service.DeleteComment(commentId, userId, isAdmin);
            return ResponseHandler(response);
        }
    }
}
