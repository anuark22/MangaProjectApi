using Microsoft.AspNetCore.Mvc;
using Models;
using Services.Interfaces;
using MangaProjectApi.CustomMiddleware;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MangaProjectApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class UsersController : HandlerController
    {
        private readonly IUsersService _usersService;

        public UsersController(IUsersService usersService)
        {
            _usersService = usersService;
        }

        [HttpGet]
        [AdminOnly]
        public IActionResult GetUsers(int? userId)
        {
            Response<Users> users = _usersService.GetUsers(userId);
            return ResponseHandler(users);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] Users model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<Users> users = await _usersService.InsertRegister(model);
            return ResponseHandler(users);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login([FromBody] Models.UsersDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<UsersDTO> users = _usersService.Login(model);
            return ResponseHandler(users);
        }

        [HttpGet]
        public IActionResult GetMyProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { Status = 401, ErrorDescription = "Token inválido o usuario no identificado." });
            }

            Response<UserProfileDTO> response = _usersService.GetUserProfile(userId);
            return ResponseHandler(response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO model)
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

            Response<Users> response = await _usersService.UpdateProfile(model, userId);
            return ResponseHandler(response);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO model)
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

            Response<Users> response = await _usersService.ChangePassword(model, userId);
            return ResponseHandler(response);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<Users> response = await _usersService.ForgotPassword(model);
            return ResponseHandler(response);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ValidateRecoveryToken([FromBody] ValidateTokenDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<Users> response = _usersService.ValidateRecoveryToken(model);
            return ResponseHandler(response);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<Users> response = await _usersService.ResetPassword(model);
            return ResponseHandler(response);
        }

        [HttpPut]
        [AdminOnly]
        public async Task<IActionResult> UpdateUserRole([FromBody] UpdateUserRoleDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            int adminUserId = userIdClaim != null && int.TryParse(userIdClaim.Value, out int id) ? id : 0;

            Response<DBClass> response = await _usersService.UpdateUserRole(model, adminUserId);
            return ResponseHandler(response);
        }
    }
}
