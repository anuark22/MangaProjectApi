using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Genres
    {
        public int GenreId { get; set; }

        public string GenreName { get; set; } = string.Empty;

        public string GenreDescription { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public bool Status { get; set; } // BIT mapea a bool

        public string GenreImgUrl { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; }
    }
    public class GenreCreateDTO
    {
        public string GenreName { get; set; } = string.Empty;

        public string GenreDescription { get; set; } = string.Empty;

        //[Required(ErrorMessage = "La imagen es obligatoria")]
        public IFormFile File { get; set; } = null!;
    }
    public class GenreUpdateDTO
    {
        public int GenreId { get; set; }

        public string? GenreName { get; set; }

        public string? GenreDescription { get; set; }

        public IFormFile? File { get; set; }
    }
}