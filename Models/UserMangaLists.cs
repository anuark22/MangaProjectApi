using System;

namespace Models
{
    public class UserMangaLists
    {
        public int UserListId { get; set; }
        public int UserId { get; set; }
        public int MangaId { get; set; }
        public string ReadingStatus { get; set; } = "PlanToRead";
        public DateTime AddedAt { get; set; }

        // Datos del manga incluidos en el JOIN
        public string MangaTittle { get; set; } = string.Empty;
        public string MangaDescription { get; set; } = string.Empty;
        public string MangaFrontImgUrl { get; set; } = string.Empty;
        public string Mangaka { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public string MangaStatus { get; set; } = string.Empty;
        public string MangaTypeName { get; set; } = string.Empty;
        public string DemographyName { get; set; } = string.Empty;

        // Aliases de conveniencia para React
        public string MangaName => MangaTittle;
        public string MangaImage => MangaFrontImgUrl;
        public string MangaType => MangaTypeName;
        public string MangaDemography => DemographyName;
        public string Synopsis => MangaDescription;
        public string Author => Mangaka;
    }

    public class AddUserMangaDTO
    {
        public int MangaId { get; set; }
        public string ReadingStatus { get; set; } = "PlanToRead";
    }

    public class UserMangaStatusCheckDTO
    {
        public int UserListId { get; set; }
        public int UserId { get; set; }
        public int MangaId { get; set; }
        public string ReadingStatus { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; }
    }
}
