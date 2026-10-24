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

        public MangaChaptersService(ICommonService commonService, IWebHostEnvironment env, IConfiguration config)
        {
            _commonService = commonService;
            _env = env;
            _config = config;
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

                // 3. Procesar y guardar físicamente las páginas en la carpeta "Files/Chapters"
                string folderPath = Path.Combine(_env.ContentRootPath, "Files", "Chapters");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                int pageNumber = 1;
                foreach (var file in model.Files)
                {
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();
                    string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                    string fullFilePath = Path.Combine(folderPath, uniqueFileName);

                    using (var stream = new FileStream(fullFilePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // 4. Registrar cada página en la tabla MangaPages
                    object pageParam = new
                    {
                        ChapterId = newChapterId,
                        PageNumber = pageNumber++,
                        PageImgUrl = $"Chapters/{uniqueFileName}"
                    };

                    // Puedes ejecutar una inserción directa con _commonService
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