namespace WebCrawler.Models
{
    public class SearchHistory
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public DateTime Date { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public ICollection<PdfDocument> Pdfs { get; set; }
    }

}
