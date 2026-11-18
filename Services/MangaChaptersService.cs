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
    public class MangaChaptersService : IMangaChaptersService
    {
        private readonly ICommonService _commonService;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly IImageStorageService _imageStorageService;

        public MangaChaptersService(ICommonService commonService, IWebHostEnvironment env, IConfiguration config, IImageStorageService imageStorageService)
        {
            _commonService = commonService;
            _env = env;
            _config = config;
            _imageStorageService = imageStorageService;
        }

        public Response<MangaChapters> GetMangaChapters(int? chapterId = null, int? mangaId = null)
        {
            Response<MangaChapters> response = new();
            int action = 1;
            object param = new { Action = action, ChapterId = chapterId, MangaId = mangaId };

            response.Items = _commonService.Query<MangaChapters>("MangaChaptersProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);

            return response;
        }

        public async Task<Response<MangaPages>> GetChapterPages(int chapterId)
        {
            Response<MangaPages> response = new();
            try
            {
                var pages = await _commonService.QueryAsync<MangaPages>(
                    "SELECT [PageId], [ChapterId], [PageNumber], [PageImgUrl] FROM [dbo].[MangaPages] WHERE [ChapterId] = @ChapterId ORDER BY [PageNumber] ASC",
                    new { ChapterId = chapterId },
                    CommandType.Text
                );

                var domainUrl = _config["MultimediaBaseUrl"] ?? _config["DomainSettings:ImageUrl"] ?? "https://localhost:44311/Files/";
                var list = pages.ToList();
                foreach (var page in list)
                {
                    if (!string.IsNullOrEmpty(page.PageImgUrl) && !page.PageImgUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        page.PageImgUrl = $"{domainUrl.TrimEnd('/')}/{page.PageImgUrl.TrimStart('/')}";
                    }
                }

                response.Items = list;
                response.ParseResponse(true);
            }
            catch (Exception ex)
            {
                response.ParseExecution(500, $"Error al obtener las páginas del capítulo: {ex.Message}");
            }

            return response;
        }

        public async Task<Response<MangaChapters>> CreateMangaChapter(MangaChapterCreateDTO model, int userId)
        {
            Response<MangaChapters> response = new();

            if (model.Files == null || model.Files.Count == 0)
            {
                response.ParseExecution(400, "Debe adjuntar las páginas del capítulo.");
                return response;
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".svg", ".webp" };

            // 1. Validar extensiones de todas las páginas antes de procesar
            foreach (var file in model.Files)
            {
                var ext = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(ext))
                {
                    response.ParseExecution(400, $"Formato de imagen no permitido: {file.FileName}. Use .jpg, .png o .webp");
                    return response;
                }
            }

            try
            {
                // 2. Insertar cabecera del capítulo en la BD usando dynamic para capturar el NewChapterId
                int action = 2;
                object param = new
                {
                    Action = action,
                    model.ChapterNumber,
                    model.ChapterTitle,
                    model.MangaId,
                    UploadedBy = userId,
                    model.ScanId,
                    model.ChapterStatus
                };

                // Cambiamos DBClass por dynamic para leer cualquier columna devuelta por el SP
                var dbResponse = await _commonService.QueryAsync<dynamic>("MangaChaptersProcedures", param, CommandType.StoredProcedure);
                var firstResult = dbResponse.FirstOrDefault();

                if (firstResult == null)
                {
                    response.ParseExecution(500, "Unexpected database error");
                    return response;
                }

                // Leemos el estatus y el error de forma segura desde el objeto dinámico
                int dbStatus = Convert.ToInt32(firstResult.Status);
                string dbError = firstResult.ErrorDescription?.ToString() ?? string.Empty;

                if (dbStatus != 200)
                {
                    response.ParseExecution(dbStatus, dbError);
                    return response;
                }

                // Capturamos correctamente el ID generado por SCOPE_IDENTITY()
                int newChapterId = Convert.ToInt32(firstResult.NewChapterId);

                // 3. Procesar, optimizar a .webp y guardar las páginas en la subcarpeta "Files/Chapters"
                int pageNumber = 1;
                foreach (var file in model.Files)
                {
                    string uniqueFileName = await _imageStorageService.SaveImageAsWebpAsync(file, "Chapters", quality: 80);

                    // 4. Registrar cada página en la tabla MangaPages
                    object pageParam = new
                    {
                        ChapterId = newChapterId,
                        PageNumber = pageNumber++,
                        PageImgUrl = $"Chapters/{uniqueFileName}"
                    };

                    await _commonService.QueryAsync<dynamic>("INSERT INTO [dbo].[MangaPages] ([ChapterId], [PageNumber], [PageImgUrl]) VALUES (@ChapterId, @PageNumber, @PageImgUrl)", pageParam, CommandType.Text);
                }

                response.ParseExecution(200, string.Empty);
            }
            catch (Exception ex)
            {
                response.ParseExecution(500, $"Error procesando el capítulo y sus páginas: {ex.Message}");
            }

            return response;
        }

        public async Task<Response<MangaChapters>> UpdateMangaChapter(MangaChapterUpdateDTO model)
        {
            Response<MangaChapters> response = new();
            int action = 3;
            object param = new
            {
                Action = action,
                model.ChapterId,
                model.ChapterNumber,
                model.ChapterTitle,
                model.ChapterStatus
            };

            try
            {
                IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("MangaChaptersProcedures", param, CommandType.StoredProcedure);
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
                response.ParseExecution(500, $"Error al actualizar el capítulo: {ex.Message}");
            }

            return response;
        }

        public async Task<Response<MangaChapters>> DeleteMangaChapter(int chapterId)
        {
            Response<MangaChapters> response = new();
            int action = 4;
            object param = new { Action = action, ChapterId = chapterId };

            try
            {
                IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("MangaChaptersProcedures", param, CommandType.StoredProcedure);
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
                response.ParseExecution(500, $"Error al eliminar el capítulo: {ex.Message}");
            }

            return response;
        }
    }
}