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
            });

            return response;
        }

        public async Task<Response<Notices>> CreateNotice(NoticeCreateDTO model, int userId)
        {
            Response<Notices> response = new();
            string? uniqueFileName = null;

            if (model.File != null && model.File.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".svg", ".webp" };
                var fileExtension = Path.GetExtension(model.File.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    response.ParseExecution(400, "Solo se permiten imágenes con formato .jpg, .png, .svg y .webp");
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
                    response.ParseExecution(500, $"Error procesando el archivo en el servidor: {ex.Message}");
                    return response;
                }
            }

            try
            {
                int action = 2;
                object param = new
                {
                    Action = action,
                    model.Title,
                    model.Content,
                    NoticeImgUrl = uniqueFileName,
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
            string? uniqueFileName = null;

            if (model.File != null && model.File.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".svg", ".webp" };
                var fileExtension = Path.GetExtension(model.File.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    response.ParseExecution(400, "Solo se permiten imágenes con formato .jpg, .png, .svg y .webp");
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
                    response.ParseExecution(500, $"Error procesando el nuevo archivo: {ex.Message}");
                    return response;
                }
            }

            try
            {
                int action = 3;
                object param = new
                {
                    Action = action,
                    model.NoticeId,
                    model.Title,
                    model.Content,
                    NoticeImgUrl = uniqueFileName,
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
    }
}
