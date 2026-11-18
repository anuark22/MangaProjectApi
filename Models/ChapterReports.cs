using System;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class ChapterReport
    {
        public int ReportId { get; set; }
        public int? ChapterId { get; set; }
        public int ChapterNumber { get; set; }
        public string? ChapterTitle { get; set; }
        public int MangaId { get; set; }
        public string MangaTittle { get; set; } = string.Empty;
        public int ScanId { get; set; }
        public string ScanName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string ReporterUsername { get; set; } = string.Empty;
        public string ReporterEmail { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = "PENDING";
        public string? AdminNotes { get; set; }
        public string? ActionTaken { get; set; }
        public int? ReviewedBy { get; set; }
        public string? ReviewerUsername { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool ChapterExists { get; set; }
    }

    public class CreateChapterReportDTO
    {
        [Required]
        public int ChapterId { get; set; }

        [Required(ErrorMessage = "Debes seleccionar un motivo de reporte.")]
        [StringLength(100)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public string? Honeypot { get; set; }
    }

    public class ReviewChapterReportDTO
    {
        [Required]
        public int ReportId { get; set; }

        [Required]
        public string Status { get; set; } = "RESOLVED"; // 'RESOLVED' | 'DISMISSED'

        public string ActionTaken { get; set; } = "NONE"; // 'NONE' | 'DELETE_CHAPTER' | 'HIDE_CHAPTER' | 'SCAN_WARNED'

        [StringLength(1000)]
        public string? AdminNotes { get; set; }
    }
}
