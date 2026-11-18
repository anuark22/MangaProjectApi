using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IImageStorageService
    {
        /// <summary>
        /// Procesa un archivo de imagen entrante (JPG, PNG, GIF, BMP, etc.), lo optimiza y lo guarda en formato .webp.
        /// Retorna el nombre único generado para el archivo (ej. "guid.webp").
        /// </summary>
        Task<string> SaveImageAsWebpAsync(IFormFile file, string subFolder = "", int quality = 82);

        /// <summary>
        /// Convierte un archivo existente en el disco al formato .webp optimizado.
        /// </summary>
        Task<string> ConvertFileToWebpAsync(string sourceFilePath, int quality = 82, bool deleteOriginal = false);

        /// <summary>
        /// Elimina un archivo físico del almacenamiento si existe.
        /// </summary>
        Task DeleteFileAsync(string relativePath);
    }
}
