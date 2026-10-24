using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Models;
using Services.Interfaces;
using MangaProjectApi.CustomMiddleware;
using System.Threading.Tasks;

namespace MangaProjectApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class DemographicsController : HandlerController
    {
        private readonly IDemographicsService _demographicsService;
        private readonly IWebHostEnvironment _env;

        public DemographicsController(IDemographicsService demographicsService, IWebHostEnvironment env)
        {
            _demographicsService = demographicsService;
            _env = env;
        }

        // =========================================================================
        // GET: Demographics/GetDemographics (PÚBLICO - No requiere Token)
        // =========================================================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetDemographics(int? demographyId, string? name)
        {
            Response<Demographics> response = _demographicsService.GetDemographics(demographyId, name);
            return ResponseHandler(response);
        }

        // =========================================================================
        // POST: Demographics/CreateDemographic (Controlador ultraligero)
        // =========================================================================
        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> CreateDemographic([FromForm] DemographicCreateDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<Demographics> response = await _demographicsService.CreateDemographic(model);
            return ResponseHandler(response);
        }

        // =========================================================================
        // PUT: Demographics/UpdateDemographic (Recibe [FromForm] para soportar cambio de imagen opcional)
        // =========================================================================
        [HttpPut]
        [AdminOnly]
        public async Task<IActionResult> UpdateDemographic([FromForm] DemographicUpdateDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<Demographics> response = await _demographicsService.UpdateDemographic(model);
            return ResponseHandler(response);
        }

        // =========================================================================
        // DELETE: Demographics/DeleteDemographic (PROTEGIDO - Requiere Token / Admin)
        // =========================================================================
        [HttpDelete]
        [AdminOnly]
        public async Task<IActionResult> DeleteDemographic(int demographyId)
        {
            Response<Demographics> response = await _demographicsService.DeleteDemographic(demographyId);
            return ResponseHandler(response);
        }
    }
}