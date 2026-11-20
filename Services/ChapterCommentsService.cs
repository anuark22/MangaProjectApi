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
    public class ChapterCommentsService : IChapterCommentsService
    {
        private readonly ICommonService _commonService;
        private readonly IImageStorageService _imageStorageService;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        private const long MaxGifSizeBytes = 5 * 1024 * 1024; // 5 MB

        public ChapterCommentsService(
            ICommonService commonService,
            IImageStorageService imageStorageService,
            IWebHostEnvironment env,
            IConfiguration config)
        {
            _commonService = commonService;
            _imageStorageService = imageStorageService;
            _env = env;
            _config = config;
        }

        public Response<ChapterComment> GetComments(int chapterId)
        {
            Response<ChapterComment> response = new();
            object param = new
            {
                Action = 1,
                ChapterId = chapterId
            };

            response.Items = _commonService.Query<ChapterComment>("ChapterCommentsProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);

            NormalizeMediaUrls(response.Items);
            return response;
        }

        public async Task<Response<ChapterComment>> AddComment(CreateChapterCommentDTO model, int userId)
        {
            Response<ChapterComment> response = new();
            string? finalMediaUrl = null;

            // 1. Validar que el comentario tenga texto o una imagen/GIF
            bool hasText = !string.IsNullOrWhiteSpace(model.Content);
            bool hasFile = model.MediaFile != null && model.MediaFile.Length > 0;
            bool hasExternalUrl = !string.IsNullOrWhiteSpace(model.MediaUrl);

            if (!hasText && !hasFile && !hasExternalUrl)
            {
                response.ParseExecution(400, "Debes escribir un comentario o adjuntar una imagen/GIF.");
                return response;
            }

            // 2. Procesar archivo adjunto (GIF animado o imagen estática)
            if (hasFile)
            {
                try
                {
                    finalMediaUrl = await SaveCommentMediaAsync(model.MediaFile!);
                }
                catch (Exception ex)
                {
                    response.ParseExecution(400, $"Error al procesar la imagen/GIF: {ex.Message}");
                    return response;
                }
            }
            else if (hasExternalUrl)
            {
                string trimmedUrl = model.MediaUrl!.Trim();
                if (!Uri.TryCreate(trimmedUrl, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                {
                    response.ParseExecution(400, "La URL de la imagen o GIF debe comenzar con http:// o https://");
                    return response;
                }
                finalMediaUrl = trimmedUrl;
            }

            object param = new
            {
                Action = 2,
                ChapterId = model.ChapterId,
                UserId = userId,
                ParentCommentId = model.ParentCommentId,
                Content = model.Content,
                MediaUrl = finalMediaUrl
            };

            var rows = await _commonService.QueryAsync<ChapterComment>("ChapterCommentsProcedures", param, CommandType.StoredProcedure);
            var list = rows.ToList();

            if (list.Count == 0)
            {
                response.ParseExecution(500, "Error inesperado al guardar el comentario.");
                return response;
            }

            // Si el SP devolvió un error (Status != 0 / != 200 en DBClass o CommentId == 0)
            var first = list[0];
            if (first.CommentId == 0)
            {
                // Reconsultar como DBClass para leer Status y ErrorDescription si falló validación SQL
                var errRows = await _commonService.QueryAsync<DBClass>("ChapterCommentsProcedures", param, CommandType.StoredProcedure);
                var err = errRows.FirstOrDefault();
                response.ParseExecution(err?.Status ?? 400, err?.ErrorDescription ?? "No se pudo guardar el comentario.");
                return response;
            }

            NormalizeMediaUrls(list);
            response.Items = list;
            response.ParseResponse(true);
            return response;
        }

        public Response<ChapterComment> DeleteComment(int commentId, int userId, bool isAdmin)
        {
            Response<ChapterComment> response = new();
            object param = new
            {
                Action = 3,
                CommentId = commentId,
                UserId = userId,
                IsAdmin = isAdmin
            };

            var dbResponse = _commonService.Query<DBClass>("ChapterCommentsProcedures", param, CommandType.StoredProcedure);
            var first = dbResponse.FirstOrDefault();

            if (first != null)
            {
                response.ParseExecution(first.Status, first.ErrorDescription);
            }
            else
            {
                response.ParseExecution(500, "Error al eliminar el comentario.");
            }

            return response;
        }

        private async Task<string> SaveCommentMediaAsync(IFormFile file)
        {
            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            // Si es GIF, validamos cabecera mágica GIF87a/GIF89a para preservar todos los frames animados
            if (ext == ".gif")
            {
                if (file.Length > MaxGifSizeBytes)
                {
                    throw new InvalidOperationException("El archivo GIF no puede superar los 5 MB.");
                }

                byte[] header = new byte[6];
                using (var stream = file.OpenReadStream())
                {
                    int read = await stream.ReadAsync(header, 0, 6);
                    if (read < 6) throw new InvalidOperationException("Archivo GIF inválido.");
                }

                string sig = System.Text.Encoding.ASCII.GetString(header);
                if (sig != "GIF87a" && sig != "GIF89a")
                {
                    throw new InvalidOperationException("El archivo no corresponde a un GIF válido.");
                }

                string filesFolder = Path.Combine(_env.ContentRootPath, "Files");
                if (!Directory.Exists(filesFolder))
                {
                    Directory.CreateDirectory(filesFolder);
                }

                string safeName = $"comment_{Guid.NewGuid():N}.gif";
                string fullPath = Path.Combine(filesFolder, safeName);

                using (var fs = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(fs);
                }

                return safeName;
            }

            // Cualquier otra imagen estática se recodifica de forma segura a WebP con SkiaSharp
            return await _imageStorageService.SaveImageAsWebpAsync(file);
        }

        private void NormalizeMediaUrls(IEnumerable<ChapterComment>? items)
        {
            if (items == null) return;
            string baseUrl = _config["MultimediaBaseUrl"] ?? string.Empty;

            foreach (var item in items)
            {
                if (!string.IsNullOrWhiteSpace(item.MediaUrl) &&
                    !item.MediaUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    item.MediaUrl = $"{baseUrl.TrimEnd('/')}/{item.MediaUrl.TrimStart('/')}";
                }
            }
        }
    }
}
