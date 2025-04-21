using Microsoft.AspNetCore.Mvc;

namespace WebCrawler.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CrawlerController : ControllerBase
    {
        [HttpGet("start")]
        public async Task<IActionResult> StartCrawl(string url, int depth = 1)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                return BadRequest("Ungültige URL.");

            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return BadRequest("URL konnte nicht aufgerufen werden.");

                var visited = new HashSet<string>();
                await CrawlAsync(url, depth, visited);

                return Ok(new { Message = "Crawling abgeschlossen", Anzahl = visited.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Fehler: {ex.Message}");
            }
        }
        private async Task CrawlAsync(string url, int depth, HashSet<string> visited)
        {
            if (depth == 0 || visited.Contains(url)) return;
            visited.Add(url);

            using var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);

            // HTML parsen mit HtmlAgilityPack
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var baseUri = new Uri(url);
            var links = doc.DocumentNode.SelectNodes("//a[@href]")
                         ?.Select(a => a.GetAttributeValue("href", ""))
                         ?.Where(href => href.StartsWith("http"))
                         ?.Distinct() ?? Enumerable.Empty<string>();

            foreach (var link in links)
            {
                var sameDomain = new Uri(link).Host == baseUri.Host;
                if ((depth == 1) ||
                    (depth == 2 && sameDomain) ||
                    (depth == 3)) // auch andere Domains
                {
                    await CrawlAsync(link, depth - 1, visited);
                }
            }
        }
    }

}
