using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class ChapterComment
    {
        public int CommentId { get; set; }
        public int ChapterId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public int? ParentCommentId { get; set; }
        public string? Content { get; set; }
        public string? MediaUrl { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateChapterCommentDTO
    {
        [Required]
        public int ChapterId { get; set; }

        public int? ParentCommentId { get; set; }

        [StringLength(2000)]
        public string? Content { get; set; }

        [StringLength(500)]
        public string? MediaUrl { get; set; }

        public IFormFile? MediaFile { get; set; }

        public string? Honeypot { get; set; }
    }
}
