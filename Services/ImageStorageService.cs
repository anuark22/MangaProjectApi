using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class ImageStorageService : IImageStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ImageStorageService> _logger;

        private static readonly string[] AllowedExtensions = new[]
        {
            ".jpg", ".jpeg", ".png", ".webp", ".bmp", ".jfif", ".gif", ".tif", ".tiff"
        };

        public ImageStorageService(IWebHostEnvironment env, ILogger<ImageStorageService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<string> SaveImageAsWebpAsync(IFormFile file, string subFolder = "", int quality = 82)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("El archivo proporcionado está vacío o no es válido.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"Formato '{extension}' no permitido. Se aceptan: {string.Join(", ", AllowedExtensions)}");
            }

            string targetFolder = string.IsNullOrWhiteSpace(subFolder)
                ? Path.Combine(_env.ContentRootPath, "Files")
                : Path.Combine(_env.ContentRootPath, "Files", subFolder);

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            string uniqueFileName = $"{Guid.NewGuid()}.webp";
            string targetFilePath = Path.Combine(targetFolder, uniqueFileName);

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using var originalBitmap = SKBitmap.Decode(memoryStream);
                if (originalBitmap == null)
                {
                    throw new InvalidOperationException("No se pudo procesar la imagen enviada. Asegúrese de que sea un archivo de imagen válido.");
                }

                using var image = SKImage.FromBitmap(originalBitmap);
                using var webpData = image.Encode(SKEncodedImageFormat.Webp, quality);

                using var fileStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                webpData.SaveTo(fileStream);
            }

            return uniqueFileName;
        }

        public Task<string> ConvertFileToWebpAsync(string sourceFilePath, int quality = 82, bool deleteOriginal = false)
        {
            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException($"El archivo no existe: {sourceFilePath}");
            }

            string dir = Path.GetDirectoryName(sourceFilePath) ?? string.Empty;
            string baseName = Path.GetFileNameWithoutExtension(sourceFilePath);
            string targetFilePath = Path.Combine(dir, $"{baseName}.webp");

            using (var originalBitmap = SKBitmap.Decode(sourceFilePath))
            {
                if (originalBitmap == null)
                {
                    throw new InvalidOperationException($"No se pudo decodificar el archivo para conversión: {sourceFilePath}");
                }

                using var image = SKImage.FromBitmap(originalBitmap);
                using var webpData = image.Encode(SKEncodedImageFormat.Webp, quality);

                using var fileStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                webpData.SaveTo(fileStream);
            }

            if (deleteOriginal && !string.Equals(sourceFilePath, targetFilePath, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    File.Delete(sourceFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo eliminar el archivo original luego de la conversión: {SourcePath}", sourceFilePath);
                }
            }

            return Task.FromResult(Path.GetFileName(targetFilePath));
        }

        public Task DeleteFileAsync(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return Task.CompletedTask;

            string normalized = relativePath.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
            string fullPath = Path.Combine(_env.ContentRootPath, "Files", normalized);

            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al eliminar archivo {Path}", fullPath);
                }
            }

            return Task.CompletedTask;
        }
    }
}
