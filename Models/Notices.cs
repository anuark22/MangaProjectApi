using Microsoft.AspNetCore.Http;
using System;

namespace Models
{
    public class Notices
    {
        public int NoticeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = "Manga";
        public string? NoticeImgUrl { get; set; }
        public string? ExtraImgUrl { get; set; }
        public string? ImageCaption { get; set; }
        public string? Quote { get; set; }
        public bool IsFeatured { get; set; }
        public string ReadTime { get; set; } = "2 min";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int CreatedBy { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        // Aliases para compatibilidad con las clases y nombres usados en NoticeCard.jsx
        public string NoticeTittle => Title;
        public string NoticeContent => Content;
        public string? ImageUrl => NoticeImgUrl;
    }

    public class NoticeCreateDTO
    {
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = "Manga";
        public IFormFile? File { get; set; }
        public IFormFile? ExtraFile { get; set; }
        public string? ImageCaption { get; set; }
        public string? Quote { get; set; }
        public bool IsFeatured { get; set; } = false;
        public string ReadTime { get; set; } = "2 min";
        public bool IsActive { get; set; } = true;
    }

    public class NoticeUpdateDTO
    {
        public int NoticeId { get; set; }
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public string? Content { get; set; }
        public string? Category { get; set; }
        public IFormFile? File { get; set; }
        public IFormFile? ExtraFile { get; set; }
        public string? ImageCaption { get; set; }
        public string? Quote { get; set; }
        public bool? IsFeatured { get; set; }
        public string? ReadTime { get; set; }
        public bool? IsActive { get; set; }
    }
}
