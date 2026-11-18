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
        private readonly IImageStorageService _imageStorageService;

        public MangasService(ICommonService commonService, IWebHostEnvironment env, IConfiguration config, IImageStorageService imageStorageService)
        {
            _commonService = commonService;
            _env = env;
            _config = config;
            _imageStorageService = imageStorageService;
        }

        public Response<Mangas> GetMangas(
            int? mangaId = null, 
            string? mangaTittle = null, 
            int? mangaTypeId = null,
            int? demographyId = null,
            string? status = null,
            bool? isAmateur = null,
            bool? isErotic = null,
            bool? isFeatured = null)
        {
            Response<Mangas> response = new();
            int action = 1;
            object param = new 
            { 
                Action = action, 
                MangaId = mangaId, 
                MangaTittle = mangaTittle, 
                MangaTypeId = mangaTypeId,
                DemographyId = demographyId,
                Status = status,
                IsAmateur = isAmateur,
                IsErotic = isErotic,
                IsFeatured = isFeatured
            };

            response.Items = _commonService.Query<Mangas>("MangasProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);

            string baseUrl = _config["MultimediaBaseUrl"] ?? string.Empty;

            Parallel.ForEach(response.Items, new ParallelOptions { MaxDegreeOfParallelism = 4 }, manga =>
            {
                if (!string.IsNullOrEmpty(manga.MangaFrontImgUrl) && !manga.MangaFrontImgUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    manga.MangaFrontImgUrl = $"{baseUrl.TrimEnd('/')}/{manga.MangaFrontImgUrl.TrimStart('/')}";
                }
            });

            return response;
        }

        public Response<Mangas> GetPopularMangas(int top = 10)
        {
            Response<Mangas> response = new();
            int action = 5;
            object param = new { Action = action };

            response.Items = _commonService.Query<Mangas>("MangasProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);

            string baseUrl = _config["MultimediaBaseUrl"] ?? string.Empty;

            Parallel.ForEach(response.Items, new ParallelOptions { MaxDegreeOfParallelism = 4 }, manga =>
            {
                if (!string.IsNullOrEmpty(manga.MangaFrontImgUrl) && !manga.MangaFrontImgUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    manga.MangaFrontImgUrl = $"{baseUrl.TrimEnd('/')}/{manga.MangaFrontImgUrl.TrimStart('/')}";
                }
            });

            return response;
        }

        public Response<MangaDetailDTO> GetMangaDetail(int mangaId)
        {
            Response<MangaDetailDTO> response = new();

            // 1. Obtener información básica del manga
            var mangaResponse = GetMangas(mangaId: mangaId);
            var manga = mangaResponse.Items?.FirstOrDefault();

            if (manga == null)
            {
                response.ParseExecution(404, "Manga no encontrado");
                return response;
            }

            // 2. Obtener capítulos asociados
            object chapterParams = new { Action = 1, MangaId = mangaId };
            var chapters = _commonService.Query<MangaChapters>("MangaChaptersProcedures", chapterParams, CommandType.StoredProcedure).ToList();

            // 3. Obtener géneros asociados
            string genreSql = @"
                SELECT g.[GenreId], g.[GenreName], g.[GenreDescription], g.[CreatedAt], g.[Status], g.[GenreImgUrl], g.[UpdatedAt]
                FROM [dbo].[Genres] g
                INNER JOIN [dbo].[MangaGenres] mg ON g.[GenreId] = mg.[GenreId]
                WHERE mg.[MangaId] = @MangaId";
            var genres = _commonService.Query<Genres>(genreSql, new { MangaId = mangaId }).ToList();

            // 4. Mapear al DTO consolidado
            MangaDetailDTO detail = new()
            {
                MangaId = manga.MangaId,
                MangaTittle = manga.MangaTittle,
                MangaDescription = manga.MangaDescription,
                AlternativeNames = manga.AlternativeNames,
                Mangaka = manga.Mangaka,
                CreatedAt = manga.CreatedAt,
                UpdatedAt = manga.UpdatedAt,
                MangaFrontImgUrl = manga.MangaFrontImgUrl,
                RegisteredBy = manga.RegisteredBy,
                MangaTypeId = manga.MangaTypeId,
                MangaTypeName = manga.MangaTypeName,
                DemographyId = manga.DemographyId,
                DemographyName = manga.DemographyName,
                EditorialName = manga.EditorialName,
                Rating = manga.Rating,
                Hidden = manga.Hidden,
                Status = manga.Status,
                IsAmateur = manga.IsAmateur,
                IsErotic = manga.IsErotic,
                IsFeatured = manga.IsFeatured,
                ViewCount = manga.ViewCount,
                Chapters = chapters,
                Genres = genres
            };

            response.Items = new List<MangaDetailDTO> { detail };
            response.ParseResponse(true);

            return response;
        }

        public async Task<Response<Mangas>> CreateManga(MangaCreateDTO model, int userId)
        {
            Response<Mangas> response = new();

            try
            {
                // Guardar y convertir la portada automáticamente a .webp
                string uniqueFileName = await _imageStorageService.SaveImageAsWebpAsync(model.File);

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
                    model.DemographyId,
                    model.EditorialName,
                    model.Rating,
                    model.Hidden,
                    model.Status,
                    model.IsAmateur,
                    model.IsErotic,
                    model.IsFeatured
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
                    model.MangaId,
                    model.MangaTittle,
                    model.MangaDescription,
                    model.AlternativeNames,
                    model.Mangaka,
                    MangaFrontImgUrl = uniqueFileName,
                    model.MangaTypeId,
                    model.DemographyId,
                    model.EditorialName,
                    model.Rating,
                    model.Hidden,
                    model.Status,
                    model.IsAmateur,
                    model.IsErotic,
                    model.IsFeatured
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
