using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Models;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class MangasTypeService : IMangasTypeService
    {
        private readonly ICommonService _commonService;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public MangasTypeService(ICommonService commonService, IWebHostEnvironment env, IConfiguration config)
        {
            _commonService = commonService;
            _env = env;
            _config = config;
        }

        public Response<MangasType> GetMangasType(int? mangaTypeId = null, string? mangaTypeName = null)
        {
            Response<MangasType> response = new();
            int action = 1;
            object param = new { Action = action, MangaTypeId = mangaTypeId, MangaTypeName = mangaTypeName };

            response.Items = _commonService.Query<MangasType>("MangasTypeProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);

            string baseUrl = _config["MultimediaBaseUrl"] ?? string.Empty;

            Parallel.ForEach(response.Items, new ParallelOptions { MaxDegreeOfParallelism = 4 }, mangaType =>
            {
                if (!string.IsNullOrEmpty(mangaType.MangaTypeImgUrl))
                {
                    mangaType.MangaTypeImgUrl = $"{baseUrl.TrimEnd('/')}/{mangaType.MangaTypeImgUrl.TrimStart('/')}";
                }
            });

            return response;
        }

        public async Task<Response<MangasType>> CreateMangaType(MangaTypeCreateDTO model)
        {
            Response<MangasType> response = new();

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".svg", ".webp" };
            var fileExtension = Path.GetExtension(model.File.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                response.ParseExecution(400, "Solo se permiten imágenes con formato .jpg, .png y .svg");
                return response;
            }

            try
            {
                string folderPath = Path.Combine(_env.ContentRootPath, "Files");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                string fullFilePath = Path.Combine(folderPath, uniqueFileName);

                using (var stream = new FileStream(fullFilePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                }

                int action = 2;
                object param = new
                {
                    Action = action,
                    model.MangaTypeName,
                    MangaTypeImgUrl = uniqueFileName,
                    model.Status
                };

                IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("MangasTypeProcedures", param, CommandType.StoredProcedure);

                var firstResult = dbResponse.FirstOrDefault();
                if (firstResult != null)
                {
                    response.ParseExecution(firstResult.Status, firstResult.ErrorDescription);
                }
                else
                {
                    response.ParseExecution(500, "Unexpected database error");
                }
            }
            catch (Exception ex)
            {
                response.ParseExecution(500, $"Error procesando el archivo en el servidor: {ex.Message}");
            }

            return response;
        }

        public async Task<Response<MangasType>> UpdateMangaType(MangaTypeUpdateDTO model)
        {
            Response<MangasType> response = new();
            string? uniqueFileName = null;

            if (model.File != null && model.File.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".svg", ".webp" };
                var fileExtension = Path.GetExtension(model.File.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    response.ParseExecution(400, "Solo se permiten imágenes con formato .jpg, .png y .svg");
                    return response;
                }

                try
                {
                    string folderPath = Path.Combine(_env.ContentRootPath, "Files");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                    string fullFilePath = Path.Combine(folderPath, uniqueFileName);

                    using (var stream = new FileStream(fullFilePath, FileMode.Create))
                    {
                        await model.File.CopyToAsync(stream);
                    }
                }
                catch (Exception ex)
                {
                    response.ParseExecution(500, $"Error procesando el nuevo archivo de imagen: {ex.Message}");
                    return response;
                }
            }

            try
            {
                int action = 3;
                object param = new
                {
                    Action = action,
                    model.MangaTypeId,
                    model.MangaTypeName,
                    MangaTypeImgUrl = uniqueFileName,
                    model.Status
                };

                IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("MangasTypeProcedures", param, CommandType.StoredProcedure);

                var firstResult = dbResponse.FirstOrDefault();
                if (firstResult != null)
                {
                    response.ParseExecution(firstResult.Status, firstResult.ErrorDescription);
                }
                else
                {
                    response.ParseExecution(500, "Unexpected database error");
                }
            }
            catch (Exception ex)
            {
                response.ParseExecution(500, $"Error al actualizar en la base de datos: {ex.Message}");
            }

            return response;
        }

        public async Task<Response<MangasType>> DeleteMangaType(int mangaTypeId)
        {
            Response<MangasType> response = new();
            int action = 4;

            object param = new
            {
                Action = action,
                MangaTypeId = mangaTypeId
            };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("MangasTypeProcedures", param, CommandType.StoredProcedure);

            var firstResult = dbResponse.FirstOrDefault();
            if (firstResult != null)
            {
                response.ParseExecution(firstResult.Status, firstResult.ErrorDescription);
            }
            else
            {
                response.ParseExecution(500, "Unexpected database error");
            }

            return response;
        }
    }
}