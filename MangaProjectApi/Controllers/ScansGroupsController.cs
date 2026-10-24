using Microsoft.AspNetCore.Hosting;
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
    public class ScansGroupsController : HandlerController
    {
        private readonly IScansGroupsService _scansGroupsService;
        private readonly IWebHostEnvironment _env;

        public ScansGroupsController(IScansGroupsService scansGroupsService, IWebHostEnvironment env)
        {
            _scansGroupsService = scansGroupsService;
            _env = env;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetScansGroups(int? scanId, string? scanName)
        {
            Response<ScansGroups> response = _scansGroupsService.GetScansGroups(scanId, scanName);
            return ResponseHandler(response);
        }

        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> CreateScanGroup([FromForm] ScanGroupCreateDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Obtenemos el Id del usuario autenticado desde los Claims del Token JWT
            // (Asegúrate de cambiar "UserId" o "sub" por el nombre del claim exacto que guardas en tu Login)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { Status = 401, ErrorDescription = "Token inválido o usuario no identificado." });
            }

            // Enviamos el modelo y el ID extraído al servicio
            Response<ScansGroups> response = await _scansGroupsService.CreateScanGroup(model, userId);
            return ResponseHandler(response);
        }

        [HttpPut]
        [AdminOnly]
        public async Task<IActionResult> UpdateScanGroup([FromForm] ScanGroupUpdateDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<ScansGroups> response = await _scansGroupsService.UpdateScanGroup(model);
            return ResponseHandler(response);
        }

        [HttpDelete]
        [AdminOnly]
        public async Task<IActionResult> DeleteScanGroup(int scanId)
        {
            Response<ScansGroups> response = await _scansGroupsService.DeleteScanGroup(scanId);
            return ResponseHandler(response);
        }
    }
}