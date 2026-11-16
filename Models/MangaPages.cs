namespace Models
{
    public class MangaPages
    {
        public int PageId { get; set; }
        public int ChapterId { get; set; }
        public int PageNumber { get; set; }
        public string PageImgUrl { get; set; } = string.Empty;
    }
}
