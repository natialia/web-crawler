namespace WebCrawler.Models
{
    public class PdfDocument
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public int SearchHistoryId { get; set; }
        public SearchHistory SearchHistory { get; set; }

        public ICollection<WordStat> WordStats { get; set; } = new List<WordStat>();
    }

}
