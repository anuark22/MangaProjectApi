using Microsoft.AspNetCore.Hosting;
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
    public class MangasTypeController : HandlerController
    {
        private readonly IMangasTypeService _mangasTypeService;
        private readonly IWebHostEnvironment _env;

        public MangasTypeController(IMangasTypeService mangasTypeService, IWebHostEnvironment env)
        {
            _mangasTypeService = mangasTypeService;
            _env = env;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetMangasType(int? mangaTypeId, string? mangaTypeName)
        {
            Response<MangasType> response = _mangasTypeService.GetMangasType(mangaTypeId, mangaTypeName);
            return ResponseHandler(response);
        }

        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> CreateMangaType([FromForm] MangaTypeCreateDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<MangasType> response = await _mangasTypeService.CreateMangaType(model);
            return ResponseHandler(response);
        }

        [HttpPut]
        [AdminOnly]
        public async Task<IActionResult> UpdateMangaType([FromForm] MangaTypeUpdateDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<MangasType> response = await _mangasTypeService.UpdateMangaType(model);
            return ResponseHandler(response);
        }

        [HttpDelete]
        [AdminOnly]
        public async Task<IActionResult> DeleteMangaType(int mangaTypeId)
        {
            Response<MangasType> response = await _mangasTypeService.DeleteMangaType(mangaTypeId);
            return ResponseHandler(response);
        }
    }
}