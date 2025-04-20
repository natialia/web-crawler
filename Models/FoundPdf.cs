namespace WebCrawler.Models
{
    public class FoundPdf
    {
        public int Id { get; set; }
        public string Link { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime DownloadedAt { get; set; }

        public int SearchHistoryId { get; set; }
        public SearchHistory SearchHistory { get; set; } = null!;
    }

}
