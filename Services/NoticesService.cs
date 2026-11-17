using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
    public class NoticesService : INoticesService
    {
        private readonly ICommonService _commonService;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public NoticesService(ICommonService commonService, IWebHostEnvironment env, IConfiguration config)
        {
            _commonService = commonService;
            _env = env;
            _config = config;
        }

        private async Task<(string? fileName, string? error)> SaveUploadedFileAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return (null, null);

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".svg", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return (null, "Solo se permiten imágenes con formato .jpg, .png, .svg y .webp");
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
                    await file.CopyToAsync(stream);
                }

                return (uniqueFileName, null);
            }
            catch (Exception ex)
            {
                return (null, $"Error procesando el archivo en el servidor: {ex.Message}");
            }
        }

        public Response<Notices> GetNotices(int? noticeId = null, bool? isActive = true)
        {
            Response<Notices> response = new();
            int action = 1;
            object param = new { Action = action, NoticeId = noticeId, IsActive = isActive };

            response.Items = _commonService.Query<Notices>("NoticesProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);

            string baseUrl = _config["MultimediaBaseUrl"] ?? string.Empty;

            Parallel.ForEach(response.Items, new ParallelOptions { MaxDegreeOfParallelism = 4 }, notice =>
            {
                if (!string.IsNullOrEmpty(notice.NoticeImgUrl) && !notice.NoticeImgUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    notice.NoticeImgUrl = $"{baseUrl.TrimEnd('/')}/{notice.NoticeImgUrl.TrimStart('/')}";
                }

                if (!string.IsNullOrEmpty(notice.ExtraImgUrl) && !notice.ExtraImgUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    notice.ExtraImgUrl = $"{baseUrl.TrimEnd('/')}/{notice.ExtraImgUrl.TrimStart('/')}";
                }
            });

            return response;
        }

        public async Task<Response<Notices>> CreateNotice(NoticeCreateDTO model, int userId)
        {
            Response<Notices> response = new();

            string? uniqueMainFile = null;
            if (model.File != null && model.File.Length > 0)
            {
                var (fileRes, err) = await SaveUploadedFileAsync(model.File);
                if (err != null)
                {
                    response.ParseExecution(400, err);
                    return response;
                }
                uniqueMainFile = fileRes;
            }

            string? uniqueExtraFile = null;
            if (model.ExtraFile != null && model.ExtraFile.Length > 0)
            {
                var (extraRes, errExtra) = await SaveUploadedFileAsync(model.ExtraFile);
                if (errExtra != null)
                {
                    response.ParseExecution(400, errExtra);
                    return response;
                }
                uniqueExtraFile = extraRes;
            }

            try
            {
                int action = 2;
                object param = new
                {
                    Action = action,
                    model.Title,
                    model.Subtitle,
                    model.Content,
                    model.Category,
                    NoticeImgUrl = uniqueMainFile,
                    ExtraImgUrl = uniqueExtraFile,
                    model.ImageCaption,
                    model.Quote,
                    model.IsFeatured,
                    model.ReadTime,
                    CreatedBy = userId,
                    model.IsActive
                };

                IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("NoticesProcedures", param, CommandType.StoredProcedure);
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
                response.ParseExecution(500, $"Error al guardar la noticia: {ex.Message}");
            }

            return response;
        }

        public async Task<Response<Notices>> UpdateNotice(NoticeUpdateDTO model)
        {
            Response<Notices> response = new();

            string? uniqueMainFile = null;
            if (model.File != null && model.File.Length > 0)
            {
                var (fileRes, err) = await SaveUploadedFileAsync(model.File);
                if (err != null)
                {
                    response.ParseExecution(400, err);
                    return response;
                }
                uniqueMainFile = fileRes;
            }

            string? uniqueExtraFile = null;
            if (model.ExtraFile != null && model.ExtraFile.Length > 0)
            {
                var (extraRes, errExtra) = await SaveUploadedFileAsync(model.ExtraFile);
                if (errExtra != null)
                {
                    response.ParseExecution(400, errExtra);
                    return response;
                }
                uniqueExtraFile = extraRes;
            }

            try
            {
                int action = 3;
                object param = new
                {
                    Action = action,
                    model.NoticeId,
                    model.Title,
                    model.Subtitle,
                    model.Content,
                    model.Category,
                    NoticeImgUrl = uniqueMainFile,
                    ExtraImgUrl = uniqueExtraFile,
                    model.ImageCaption,
                    model.Quote,
                    model.IsFeatured,
                    model.ReadTime,
                    model.IsActive
                };

                IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("NoticesProcedures", param, CommandType.StoredProcedure);
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
                response.ParseExecution(500, $"Error al actualizar la noticia: {ex.Message}");
            }

            return response;
        }

        public async Task<Response<Notices>> DeleteNotice(int noticeId)
        {
            Response<Notices> response = new();
            int action = 4;
            object param = new { Action = action, NoticeId = noticeId };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("NoticesProcedures", param, CommandType.StoredProcedure);
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

        public async Task<Response<Notices>> ToggleNoticeActive(int noticeId)
        {
            Response<Notices> response = new();
            int action = 5;
            object param = new { Action = action, NoticeId = noticeId };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("NoticesProcedures", param, CommandType.StoredProcedure);
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

        public async Task<Response<Notices>> ToggleNoticeFeatured(int noticeId)
        {
            Response<Notices> response = new();
            int action = 6;
            object param = new { Action = action, NoticeId = noticeId };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("NoticesProcedures", param, CommandType.StoredProcedure);
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
