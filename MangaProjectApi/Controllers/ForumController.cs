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

        public ForumController(IForumService forumService)
        {
            _forumService = forumService;
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

            Response<ForumPosts> response = await _forumService.CreatePost(model, userId);
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

            Response<ForumComments> response = await _forumService.AddComment(model, userId);
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
