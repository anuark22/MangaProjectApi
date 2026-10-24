using System;

namespace Models
{
    public class Manga
    {
        public int MangaId { get; set; }

        public string MangaTittle { get; set; } = string.Empty;

        public string MangaDescription { get; set; } = string.Empty;

        public string AlternativeNames { get; set; } = string.Empty;

        public string Mangaka { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; } // Nullable ya que en la BD acepta NULL

        public string MangaFrontImgUrl { get; set; } = string.Empty;

        public int RegisteredBy { get; set; }

        public int MangaTypeId { get; set; }

        public string EditorialName { get; set; } = string.Empty;

        public decimal Rating { get; set; }

        public bool Hidden { get; set; } // BIT mapea directamente a bool en C#
    }
}