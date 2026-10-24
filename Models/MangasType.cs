using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class MangasType
    {
        public int MangaTypeId { get; set; }
        public string MangaTypeName { get; set; } = string.Empty;
        public string MangaTypeImgUrl { get; set; } = string.Empty;
        public bool Status { get; set; }
    }

    public class MangaTypeCreateDTO
    {
        public string MangaTypeName { get; set; } = string.Empty;

        public bool Status { get; set; } = true;

        public IFormFile File { get; set; } = null!;
    }

    public class MangaTypeUpdateDTO
    {
        public int MangaTypeId { get; set; }

        public string? MangaTypeName { get; set; }

        public bool? Status { get; set; }

        // Opcional al actualizar (si no se envía, se conserva la anterior)
        public IFormFile? File { get; set; }
    }
}