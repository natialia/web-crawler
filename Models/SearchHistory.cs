namespace WebCrawler.Models
{
    public class SearchHistory
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public DateTime SearchedAt { get; set; }
        public List<FoundPdf> FoundPdfs { get; set; } = new();
    }

}
