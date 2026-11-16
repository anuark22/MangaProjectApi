using Microsoft.AspNetCore.Mvc;
using MangaProjectApi.CustomMiddleware;
using Models;
using Services.Interfaces;
using System.Threading.Tasks;

namespace MangaProjectApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    [AdminOnly]
    public class RolesController : HandlerController
    {
        private readonly IRolesService _rolesService;

        public RolesController(IRolesService rolesService)
        {
            _rolesService = rolesService;
        }

        [HttpGet]
        public IActionResult GetRoles(int? roleId, string? roleName, bool? isActive)
        {
            Response<Roles> response = _rolesService.GetRoles(roleId, roleName, isActive);
            return ResponseHandler(response);
        }

        [HttpGet]
        public IActionResult GetRoleById(int roleId)
        {
            Response<Roles> response = _rolesService.GetRoles(roleId: roleId);
            return ResponseHandler(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] RoleCreateDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<Roles> response = await _rolesService.CreateRole(model);
            return ResponseHandler(response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRole([FromBody] RoleUpdateDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<Roles> response = await _rolesService.UpdateRole(model);
            return ResponseHandler(response);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRole(int roleId)
        {
            Response<Roles> response = await _rolesService.DeleteRole(roleId);
            return ResponseHandler(response);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<DBClass> response = await _rolesService.AssignRole(model);
            return ResponseHandler(response);
        }

        [HttpGet]
        public IActionResult GetUsersByRole(int roleId)
        {
            Response<UserRoleInfoDTO> response = _rolesService.GetUsersByRole(roleId);
            return ResponseHandler(response);
        }
    }
}
