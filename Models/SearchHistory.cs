public class SearchHistory
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public List<string> PdfLinks { get; set; } = new();
    public int UserId { get; set; }
}
