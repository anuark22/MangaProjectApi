using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class ScansGroups
    {
        public int ScanId { get; set; }
        public string ScanName { get; set; } = string.Empty;
        public string ScanDescription { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        public bool EmailNotificaction { get; set; }
        public string ScanImgUrl { get; set; } = string.Empty;
        public bool ScanStatus { get; set; }
    }

    public class ScanGroupCreateDTO
    {
        public string ScanName { get; set; } = string.Empty;

        public string ScanDescription { get; set; } = string.Empty;

        public bool EmailNotificaction { get; set; }

        public bool ScanStatus { get; set; } = true;

        public IFormFile File { get; set; } = null!;
    }

    public class ScanGroupUpdateDTO
    {
        public int ScanId { get; set; }

        public string? ScanName { get; set; }

        public string? ScanDescription { get; set; }

        public bool? EmailNotificaction { get; set; }

        public bool? ScanStatus { get; set; }

        // Opcional al actualizar
        public IFormFile? File { get; set; }
    }
}