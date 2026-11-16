using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class ScanGroupApplication
    {
        public int ApplicationId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string ScanName { get; set; } = string.Empty;
        public string ScanDescription { get; set; } = string.Empty;
        public string ScanImgUrl { get; set; } = string.Empty;
        public bool EmailNotification { get; set; }
        public string? DiscordUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? ApplicationMessage { get; set; }
        public string Status { get; set; } = "PENDING"; // PENDING, APPROVED, REJECTED
        public string? AdminNotes { get; set; }
        public int? ReviewedBy { get; set; }
        public string? ReviewedByUsername { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int? CreatedScanId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ScanGroupApplyDTO
    {
        [Required(ErrorMessage = "El nombre del grupo de scan es obligatorio")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "El nombre del grupo debe tener entre 2 y 255 caracteres")]
        public string ScanName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción del grupo es obligatoria")]
        public string ScanDescription { get; set; } = string.Empty;

        public bool EmailNotification { get; set; } = false;

        public string? DiscordUrl { get; set; }

        public string? WebsiteUrl { get; set; }

        public string? ApplicationMessage { get; set; }

        // Logo o imagen representativa del scan group (opcional)
        public IFormFile? File { get; set; }
    }

    public class ReviewScanGroupApplicationDTO
    {
        [Required(ErrorMessage = "El ID de la solicitud es obligatorio")]
        public int ApplicationId { get; set; }

        [Required(ErrorMessage = "Debe indicar si aprueba o rechaza la solicitud")]
        public bool IsApproved { get; set; }

        // Notas o motivo del rechazo / observaciones del administrador
        public string? AdminNotes { get; set; }
    }
}
