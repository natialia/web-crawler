using Microsoft.AspNetCore.Identity;

namespace WebCrawler.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Nickname { get; set; }
    }
}
