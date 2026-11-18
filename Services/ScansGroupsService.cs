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
    public class ScansGroupsService : IScansGroupsService
    {
        private readonly ICommonService _commonService;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly IImageStorageService _imageStorageService;

        public ScansGroupsService(ICommonService commonService, IWebHostEnvironment env, IConfiguration config, IImageStorageService imageStorageService)
        {
            _commonService = commonService;
            _env = env;
            _config = config;
            _imageStorageService = imageStorageService;
        }

        public Response<ScansGroups> GetScansGroups(int? scanId = null, string? scanName = null)
        {
            Response<ScansGroups> response = new();
            int action = 1;
            object param = new { Action = action, ScanId = scanId, ScanName = scanName };

            response.Items = _commonService.Query<ScansGroups>("ScansGroupsProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);

            string baseUrl = _config["MultimediaBaseUrl"] ?? string.Empty;

            Parallel.ForEach(response.Items, new ParallelOptions { MaxDegreeOfParallelism = 4 }, scan =>
            {
                if (!string.IsNullOrEmpty(scan.ScanImgUrl))
                {
                    scan.ScanImgUrl = $"{baseUrl.TrimEnd('/')}/{scan.ScanImgUrl.TrimStart('/')}";
                }
            });

            return response;
        }
        // Modificación en el método CreateScanGroup:
        public async Task<Response<ScansGroups>> CreateScanGroup(ScanGroupCreateDTO model, int userId)
        {
            Response<ScansGroups> response = new();

            try
            {
                string uniqueFileName = await _imageStorageService.SaveImageAsWebpAsync(model.File);

                int action = 2;
                object param = new
                {
                    Action = action,
                    model.ScanName,
                    model.ScanDescription,
                    CreatedBy = userId,
                    model.EmailNotificaction,
                    ScanImgUrl = uniqueFileName,
                    model.ScanStatus
                };

                IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("ScansGroupsProcedures", param, CommandType.StoredProcedure);

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

        public async Task<Response<ScansGroups>> UpdateScanGroup(ScanGroupUpdateDTO model)
        {
            Response<ScansGroups> response = new();
            string? uniqueFileName = null;

            if (model.File != null && model.File.Length > 0)
            {
                try
                {
                    uniqueFileName = await _imageStorageService.SaveImageAsWebpAsync(model.File);
                }
                catch (Exception ex)
                {
                    response.ParseExecution(400, $"Error procesando el nuevo archivo de imagen: {ex.Message}");
                    return response;
                }
            }

            try
            {
                int action = 3;
                object param = new
                {
                    Action = action,
                    model.ScanId,
                    model.ScanName,
                    model.ScanDescription,
                    model.EmailNotificaction,
                    ScanImgUrl = uniqueFileName,
                    model.ScanStatus
                };

                IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("ScansGroupsProcedures", param, CommandType.StoredProcedure);

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

        public async Task<Response<ScansGroups>> DeleteScanGroup(int scanId)
        {
            Response<ScansGroups> response = new();
            int action = 4;

            object param = new
            {
                Action = action,
                ScanId = scanId
            };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("ScansGroupsProcedures", param, CommandType.StoredProcedure);

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