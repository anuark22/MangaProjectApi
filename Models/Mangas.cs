using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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
        public string MangaTypeName { get; set; } = string.Empty;
        public int? DemographyId { get; set; }
        public string DemographyName { get; set; } = string.Empty;
        public string EditorialName { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public bool Hidden { get; set; }
        public string Status { get; set; } = "Publishing";
        public bool IsAmateur { get; set; }
        public bool IsErotic { get; set; }
        public bool IsFeatured { get; set; }
        public long ViewCount { get; set; }

        // Propiedades de conveniencia alineadas con las vistas de React
        public string MangaName => MangaTittle;
        public string MangaImage => MangaFrontImgUrl;
        public string MangaType => MangaTypeName;
        public string MangaDemography => DemographyName;
        public string Synopsis => MangaDescription;
        public string Author => Mangaka;
    }

    public class MangaCreateDTO
    {
        public string MangaTittle { get; set; } = string.Empty;
        public string MangaDescription { get; set; } = string.Empty;
        public string AlternativeNames { get; set; } = string.Empty;
        public string Mangaka { get; set; } = string.Empty;
        public int MangaTypeId { get; set; }
        public int? DemographyId { get; set; }
        public string EditorialName { get; set; } = string.Empty;
        public decimal Rating { get; set; } = 0;
        public bool Hidden { get; set; } = false;
        public string Status { get; set; } = "Publishing";
        public bool IsAmateur { get; set; } = false;
        public bool IsErotic { get; set; } = false;
        public bool IsFeatured { get; set; } = false;
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
        public int? DemographyId { get; set; }
        public string? EditorialName { get; set; }
        public decimal? Rating { get; set; }
        public bool? Hidden { get; set; }
        public string? Status { get; set; }
        public bool? IsAmateur { get; set; }
        public bool? IsErotic { get; set; }
        public bool? IsFeatured { get; set; }
        public IFormFile? File { get; set; }
    }

    public class MangaDetailDTO : Mangas
    {
        public List<MangaChapters> Chapters { get; set; } = new();
        public List<Genres> Genres { get; set; } = new();
    }
}
