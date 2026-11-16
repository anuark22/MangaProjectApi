using Microsoft.AspNetCore.Http;
using System;

namespace Models
{
    public class Notices
    {
        public int NoticeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? NoticeImgUrl { get; set; }
        public DateTime CreatedAt { get; set; }
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
        public string Content { get; set; } = string.Empty;
        public IFormFile? File { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class NoticeUpdateDTO
    {
        public int NoticeId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public IFormFile? File { get; set; }
        public bool? IsActive { get; set; }
    }
}
