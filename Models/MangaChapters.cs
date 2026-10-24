using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class MangaChapters
    {
        public int ChapterId { get; set; }
        public int ChapterNumber { get; set; }
        public string? ChapterTitle { get; set; }
        public int MangaId { get; set; }
        public int UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; }
        public int ScanId { get; set; }
        public int ViewAmount { get; set; }
        public decimal Rating { get; set; }
        public bool ChapterStatus { get; set; }
    }

    public class MangaChapterCreateDTO
    {
        public int ChapterNumber { get; set; }

        public string? ChapterTitle { get; set; }

        public int MangaId { get; set; }

        public int ScanId { get; set; }

        public bool ChapterStatus { get; set; } = true;

        public List<IFormFile> Files { get; set; } = new();
    }

    public class MangaChapterUpdateDTO
    {
        public int ChapterId { get; set; }

        public int? ChapterNumber { get; set; }
        public string? ChapterTitle { get; set; }
        public bool? ChapterStatus { get; set; }
    }
}