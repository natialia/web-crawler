using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCrawler.Data;

namespace WebCrawler.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CrawlerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CrawlerController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("start")]
        public async Task<IActionResult> StartCrawl(string url, int depth = 1, int userId = 0)
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
                var foundPdfs = new List<string>();
                await CrawlAsync(url, depth, visited, foundPdfs);

                _context.SearchHistories.Add(new SearchHistory
                {
                    Url = url,
                    Date = DateTime.UtcNow,
                    PdfLinks = foundPdfs,
                    UserId = userId // wird vom Frontend übergeben
                });


                return Ok(new
                {
                    Message = "Crawling abgeschlossen",
                    AnzahlSeiten = visited.Count,
                    AnzahlPDFs = foundPdfs.Count,
                    PDFLinks = foundPdfs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Fehler: {ex.Message}");
            }
        }

        private async Task CrawlAsync(string url, int depth, HashSet<string> visited, List<string> foundPdfs)
        {
            if (depth == 0 || visited.Contains(url) || visited.Count >= 100)
                return;

            visited.Add(url);

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);

            HttpResponseMessage response;
            try
            {
                response = await httpClient.GetAsync(url);
            }
            catch
            {
                return;
            }

            if (!response.IsSuccessStatusCode ||
                !(response.Content.Headers.ContentType?.MediaType?.Contains("text/html") ?? false))
                return;

            var html = await response.Content.ReadAsStringAsync();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var baseUri = new Uri(url);
            var rawLinks = doc.DocumentNode.SelectNodes("//a[@href]") ?? new HtmlAgilityPack.HtmlNodeCollection(null);

            var links = rawLinks
                .Select(a => a.GetAttributeValue("href", ""))
                .Select(href =>
                {
                    try { return new Uri(baseUri, href).ToString(); } catch { return null; }
                })
                .Where(link => !string.IsNullOrEmpty(link) && link.StartsWith("http"))
                .Distinct();

            foreach (var link in links)
            {
                if (link.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    if (!foundPdfs.Contains(link))
                        foundPdfs.Add(link);
                }

                var sameDomain = new Uri(link).Host == baseUri.Host;
                if ((depth == 1) ||
                    (depth == 2 && sameDomain) ||
                    (depth == 3))
                {
                    await CrawlAsync(link, depth - 1, visited, foundPdfs);
                }
            }
        }

    }

}
