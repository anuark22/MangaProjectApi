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
    public class ForumController : HandlerController
    {
        private readonly IForumService _forumService;
        private readonly IAntiSpamService _antiSpamService;

        public ForumController(IForumService forumService, IAntiSpamService antiSpamService)
        {
            _forumService = forumService;
            _antiSpamService = antiSpamService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetPosts(int? postId, string? topic, string? subTopic, string sortBy = "Latest")
        {
            Response<ForumPosts> response = _forumService.GetPosts(postId, topic, subTopic, sortBy);
            return ResponseHandler(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreateForumPostDTO model)
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

            // Flood control: máximo 1 publicación cada 10 segundos por usuario
            var (isRateLimited, secondsRemaining) = _antiSpamService.CheckActionCooldown("forum_post", userId.ToString(), 10);
            if (isRateLimited)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, new
                {
                    Status = 429,
                    Error = true,
                    SecondsRemaining = secondsRemaining,
                    ErrorDescription = $"Por favor espera {secondsRemaining} segundos antes de publicar otro tema en el foro."
                });
            }

            // Detección de contenido duplicado (evita spam de temas idénticos en menos de 60s)
            if (_antiSpamService.IsDuplicateContent($"post_{userId}", $"{model.Title}:{model.Content}", 60))
            {
                return BadRequest(new
                {
                    Status = 400,
                    Error = true,
                    ErrorDescription = "Ya has publicado este mismo contenido recientemente en el foro. Evita publicaciones duplicadas."
                });
            }

            Response<ForumPosts> response = await _forumService.CreatePost(model, userId);
            if (response.Status == 200 || !response.Error)
            {
                _antiSpamService.RecordAction("forum_post", userId.ToString());
            }
            return ResponseHandler(response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePost([FromBody] UpdateForumPostDTO model)
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

            Response<ForumPosts> response = await _forumService.UpdatePost(model, userId);
            return ResponseHandler(response);
        }

        [HttpDelete]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { Status = 401, ErrorDescription = "Token inválido o usuario no identificado." });
            }

            var roleClaim = User.FindFirst(ClaimTypes.Role);
            bool isAdmin = roleClaim?.Value == "ADMIN";

            Response<ForumPosts> response = await _forumService.DeletePost(postId, userId, isAdmin);
            return ResponseHandler(response);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> IncrementViews(int postId)
        {
            Response<DBClass> response = await _forumService.IncrementViews(postId);
            return ResponseHandler(response);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetComments(int postId)
        {
            Response<ForumComments> response = _forumService.GetComments(postId);
            return ResponseHandler(response);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CreateForumCommentDTO model)
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

            // Flood control: máximo 1 comentario cada 5 segundos por usuario
            var (isRateLimited, secondsRemaining) = _antiSpamService.CheckActionCooldown("forum_comment", userId.ToString(), 5);
            if (isRateLimited)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, new
                {
                    Status = 429,
                    Error = true,
                    SecondsRemaining = secondsRemaining,
                    ErrorDescription = $"Por favor espera {secondsRemaining} segundos antes de enviar otro comentario."
                });
            }

            // Detección de comentarios duplicados idénticos en menos de 30s
            if (_antiSpamService.IsDuplicateContent($"comment_{userId}_{model.PostId}", model.Content, 30))
            {
                return BadRequest(new
                {
                    Status = 400,
                    Error = true,
                    ErrorDescription = "Ya has publicado exactamente este comentario recientemente. Evita comentarios repetidos."
                });
            }

            Response<ForumComments> response = await _forumService.AddComment(model, userId);
            if (response.Status == 200 || !response.Error)
            {
                _antiSpamService.RecordAction("forum_comment", userId.ToString());
            }
            return ResponseHandler(response);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { Status = 401, ErrorDescription = "Token inválido o usuario no identificado." });
            }

            var roleClaim = User.FindFirst(ClaimTypes.Role);
            bool isAdmin = roleClaim?.Value == "ADMIN";

            Response<ForumComments> response = await _forumService.DeleteComment(commentId, userId, isAdmin);
            return ResponseHandler(response);
        }
    }
}
