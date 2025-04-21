namespace WebCrawler.Models
{
    public class LoginDto
    {
        public string EmailOrNickname { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}