using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Mangas
    {
        public int MangaId { get; set; }
        public string MangaTittle { get; set; } = string.Empty;
        public string MangaDescription { get; set; } = string.Empty;
        public string AlternativeNames { get; set; } = string.Empty;
        public string Mangaka { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string MangaFrontImgUrl { get; set; } = string.Empty;
        public int RegisteredBy { get; set; }
        public int MangaTypeId { get; set; }
        public string EditorialName { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public bool Hidden { get; set; }
    }

    public class MangaCreateDTO
    {
        public string MangaTittle { get; set; } = string.Empty;

        public string MangaDescription { get; set; } = string.Empty;

        public string AlternativeNames { get; set; } = string.Empty;

        public string Mangaka { get; set; } = string.Empty;

        public int MangaTypeId { get; set; }

        public string EditorialName { get; set; } = string.Empty;

        public decimal Rating { get; set; } = 0;

        public bool Hidden { get; set; } = false;

        public IFormFile File { get; set; } = null!;
    }

    public class MangaUpdateDTO
    {
        public int MangaId { get; set; }

        public string? MangaTittle { get; set; }

        public string? MangaDescription { get; set; }

        public string? AlternativeNames { get; set; }

        public string? Mangaka { get; set; }

        public int? MangaTypeId { get; set; }

        public string? EditorialName { get; set; }

        public decimal? Rating { get; set; }

        public bool? Hidden { get; set; }

        // Opcional al actualizar
        public IFormFile? File { get; set; }
    }
}