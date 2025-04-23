namespace WebCrawler.Models
{
    public class WordStat
    {
        public int Id { get; set; }
        public string Word { get; set; } = "";
        public int Count { get; set; }

        public int PdfDocumentId { get; set; }
        public PdfDocument? PdfDocument { get; set; }
    }

}
