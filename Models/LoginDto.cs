namespace WebCrawler.Models
{
    public class LoginDto
    {
        public string? Email { get; set; }
        public string? Nickname { get; set; }
        public string Password { get; set; } = string.Empty;
    }
}