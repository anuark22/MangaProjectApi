using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Demographics
    {
        public int DemographyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DemographyImgUrl { get; set; } = string.Empty;
    }
    public class DemographicCreateDTO
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public IFormFile File { get; set; } = null!;
    }

    public class DemographicUpdateDTO
    {
        public int DemographyId { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public IFormFile? File { get; set; }
    }
}
