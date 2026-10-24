using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class MangaGenres
    {
        public int GenreId { get; set; }
        public int MangaId { get; set; }
    }

    public class MangaGenreCreateDTO
    {
        public int GenreId { get; set; }
        public int MangaId { get; set; }
    }
}