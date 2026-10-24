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
    public class DemographicsService : IDemographicsService
    {
        private readonly ICommonService _commonService;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public DemographicsService(ICommonService commonService, IWebHostEnvironment env, IConfiguration config)
        {
            _commonService = commonService;
            _env = env;
            _config = config;
        }

        // =========================================================================
        // ACTION 1: Obtener todas las demografías o filtrar
        // =========================================================================
        public Response<Demographics> GetDemographics(int? demographyId = null, string? name = null)
        {
            Response<Demographics> response = new();
            int action = 1;
            object param = new { Action = action, DemographyId = demographyId, Name = name };

            response.Items = _commonService.Query<Demographics>("DemographicsProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);

            string baseUrl = _config["MultimediaBaseUrl"] ?? string.Empty;

            Parallel.ForEach(response.Items, new ParallelOptions { MaxDegreeOfParallelism = 4 }, demo =>
            {
                if (!string.IsNullOrEmpty(demo.DemographyImgUrl))
                {
                    demo.DemographyImgUrl = $"{baseUrl.TrimEnd('/')}/{demo.DemographyImgUrl.TrimStart('/')}";
                }
            });

            return response;
        }

        // =========================================================================
        // ACTION 2: Crear demografía
        // =========================================================================
        public async Task<Response<Demographics>> CreateDemographic(DemographicCreateDTO model)
        {
            Response<Demographics> response = new();

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
                    model.Name,
                    model.Description,
                    DemographyImgUrl = uniqueFileName
                };

                IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("DemographicsProcedures", param, CommandType.StoredProcedure);

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

        // =========================================================================
        // ACTION 3: Actualizar demografía
        // =========================================================================
        public async Task<Response<Demographics>> UpdateDemographic(DemographicUpdateDTO model)
        {
            Response<Demographics> response = new();
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
                    model.DemographyId,
                    model.Name,
                    model.Description,
                    DemographyImgUrl = uniqueFileName
                };

                IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("DemographicsProcedures", param, CommandType.StoredProcedure);

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

        // =========================================================================
        // ACTION 4: Borrado físico
        // =========================================================================
        public async Task<Response<Demographics>> DeleteDemographic(int demographyId)
        {
            Response<Demographics> response = new();
            int action = 4;

            object param = new
            {
                Action = action,
                DemographyId = demographyId
            };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("DemographicsProcedures", param, CommandType.StoredProcedure);

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