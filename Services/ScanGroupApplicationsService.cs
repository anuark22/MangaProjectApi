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
    public class ScanGroupApplicationsService : IScanGroupApplicationsService
    {
        private readonly ICommonService _commonService;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public ScanGroupApplicationsService(ICommonService commonService, IWebHostEnvironment env, IConfiguration config)
        {
            _commonService = commonService;
            _env = env;
            _config = config;
        }

        public Response<ScanGroupApplication> GetApplications(string? status = null, int? applicationId = null)
        {
            Response<ScanGroupApplication> response = new();
            int action = 1;
            object param = new
            {
                Action = action,
                Status = string.IsNullOrWhiteSpace(status) ? null : status.Trim().ToUpper(),
                ApplicationId = applicationId
            };

            response.Items = _commonService.Query<ScanGroupApplication>("ScanGroupApplicationsProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);

            NormalizeImageUrls(response.Items);
            return response;
        }

        public Response<ScanGroupApplication> GetMyApplication(int userId)
        {
            Response<ScanGroupApplication> response = new();
            int action = 5;
            object param = new
            {
                Action = action,
                UserId = userId
            };

            response.Items = _commonService.Query<ScanGroupApplication>("ScanGroupApplicationsProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);

            NormalizeImageUrls(response.Items);
            return response;
        }

        public async Task<Response<ScanGroupApplication>> Apply(ScanGroupApplyDTO model, int userId)
        {
            Response<ScanGroupApplication> response = new();
            string fileName = "default-scan.png";

            if (model.File != null && model.File.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".svg", ".webp" };
                var fileExtension = Path.GetExtension(model.File.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    response.ParseExecution(400, "Solo se permiten imágenes con formato .jpg, .jpeg, .png, .svg y .webp");
                    return response;
                }

                try
                {
                    string folderPath = Path.Combine(_env.ContentRootPath, "Files");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    fileName = $"{Guid.NewGuid()}{fileExtension}";
                    string fullFilePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(fullFilePath, FileMode.Create))
                    {
                        await model.File.CopyToAsync(stream);
                    }
                }
                catch (Exception ex)
                {
                    response.ParseExecution(500, $"Error al guardar el archivo del logo: {ex.Message}");
                    return response;
                }
            }

            int action = 2;
            object param = new
            {
                Action = action,
                UserId = userId,
                model.ScanName,
                model.ScanDescription,
                ScanImgUrl = fileName,
                model.EmailNotification,
                model.DiscordUrl,
                model.WebsiteUrl,
                model.ApplicationMessage
            };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("ScanGroupApplicationsProcedures", param, CommandType.StoredProcedure);
            var firstResult = dbResponse.FirstOrDefault();

            if (firstResult != null)
            {
                response.ParseExecution(firstResult.Status, firstResult.ErrorDescription);
            }
            else
            {
                response.ParseExecution(500, "Error inesperado en la base de datos.");
            }

            return response;
        }

        public async Task<Response<DBClass>> ReviewApplication(ReviewScanGroupApplicationDTO model, int adminUserId)
        {
            Response<DBClass> response = new();
            int action = model.IsApproved ? 3 : 4;

            object param = new
            {
                Action = action,
                model.ApplicationId,
                model.AdminNotes,
                AdminUserId = adminUserId
            };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("ScanGroupApplicationsProcedures", param, CommandType.StoredProcedure);
            var firstResult = dbResponse.FirstOrDefault();

            if (firstResult != null)
            {
                response.ParseExecution(firstResult.Status, firstResult.ErrorDescription);
            }
            else
            {
                response.ParseExecution(500, "Error inesperado al procesar la revisión de la solicitud.");
            }

            return response;
        }

        private void NormalizeImageUrls(IEnumerable<ScanGroupApplication>? items)
        {
            if (items == null) return;

            string baseUrl = _config["MultimediaBaseUrl"] ?? string.Empty;

            Parallel.ForEach(items, new ParallelOptions { MaxDegreeOfParallelism = 4 }, app =>
            {
                if (!string.IsNullOrEmpty(app.ScanImgUrl) && !app.ScanImgUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    app.ScanImgUrl = $"{baseUrl.TrimEnd('/')}/{app.ScanImgUrl.TrimStart('/')}";
                }
            });
        }
    }
}
