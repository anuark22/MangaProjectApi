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
    public class MangasService : IMangasService
    {
        private readonly ICommonService _commonService;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public MangasService(ICommonService commonService, IWebHostEnvironment env, IConfiguration config)
        {
            _commonService = commonService;
            _env = env;
            _config = config;
        }

        public Response<Mangas> GetMangas(int? mangaId = null, string? mangaTittle = null, int? mangaTypeId = null)
        {
            Response<Mangas> response = new();
            int action = 1;
            object param = new { Action = action, MangaId = mangaId, MangaTittle = mangaTittle, MangaTypeId = mangaTypeId };

            response.Items = _commonService.Query<Mangas>("MangasProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);

            string baseUrl = _config["MultimediaBaseUrl"] ?? string.Empty;

            Parallel.ForEach(response.Items, new ParallelOptions { MaxDegreeOfParallelism = 4 }, manga =>
            {
                if (!string.IsNullOrEmpty(manga.MangaFrontImgUrl))
                {
                    manga.MangaFrontImgUrl = $"{baseUrl.TrimEnd('/')}/{manga.MangaFrontImgUrl.TrimStart('/')}";
                }
            });

            return response;
        }

        public async Task<Response<Mangas>> CreateManga(MangaCreateDTO model, int userId)
        {
            Response<Mangas> response = new();

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".svg" , ".webp" };
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
                    model.MangaTittle,
                    model.MangaDescription,
                    model.AlternativeNames,
                    model.Mangaka,
                    MangaFrontImgUrl = uniqueFileName,
                    RegisteredBy = userId,
                    model.MangaTypeId,
                    model.EditorialName,
                    model.Rating,
                    model.Hidden
                };

                IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("MangasProcedures", param, CommandType.StoredProcedure);

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

        public async Task<Response<Mangas>> UpdateManga(MangaUpdateDTO model)
        {
            Response<Mangas> response = new();
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
                    model.MangaId,
                    model.MangaTittle,
                    model.MangaDescription,
                    model.AlternativeNames,
                    model.Mangaka,
                    MangaFrontImgUrl = uniqueFileName,
                    model.MangaTypeId,
                    model.EditorialName,
                    model.Rating,
                    model.Hidden
                };

                IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("MangasProcedures", param, CommandType.StoredProcedure);

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

        public async Task<Response<Mangas>> DeleteManga(int mangaId)
        {
            Response<Mangas> response = new();
            int action = 4;

            object param = new
            {
                Action = action,
                MangaId = mangaId
            };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("MangasProcedures", param, CommandType.StoredProcedure);

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