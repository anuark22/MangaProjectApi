using Microsoft.AspNetCore.Mvc;
using Models;
using Services;
using Services.Interfaces;
using MangaProjectApi.CustomMiddleware;


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
        private readonly UsersService usersService;

        [HttpGet]
        [AdminOnly]
        public IActionResult GetUsers(int? userId)
        {
            UsersDTO? user = (UsersDTO)HttpContext.Items["Client"];
            Response<Users> users = _usersService.GetUsers(userId);
            return ResponseHandler(users);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(Users model)
        {
            Response<Users> users = await _usersService.InsertRegister(model);
            return ResponseHandler(users);
        }
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login(Models.UsersDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Response<UsersDTO> users = _usersService.Login(model);
            return ResponseHandler(users);
        }
    }
}
