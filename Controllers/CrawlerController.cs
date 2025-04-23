using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCrawler.Data;
using WebCrawler.Models;
using UglyToad.PdfPig;
using System.Net.Http;
using PdfModel = WebCrawler.Models.PdfDocument;

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
        public async Task<IActionResult> StartCrawl(string url, int depth = 1, string userId = "")
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
                var foundPdfs = new List<PdfModel>();
                await CrawlAsync(url, depth, visited, foundPdfs);

                var history = new SearchHistory
                {
                    Url = url,
                    Date = DateTime.UtcNow,
                    UserId = userId,
                    Pdfs = foundPdfs
                };

                _context.SearchHistories.Add(history);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Crawling abgeschlossen",
                    AnzahlSeiten = visited.Count,
                    AnzahlPDFs = foundPdfs.Count,
                    PDFLinks = foundPdfs.Select(p => p.FilePath)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Fehler: {ex.Message}");
            }
        }

        private async Task CrawlAsync(string url, int depth, HashSet<string> visited, List<PdfModel> foundPdfs)
        {
            if (depth == 0 || visited.Contains(url) || visited.Count >= 25)
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
                    var filename = Path.GetFileName(new Uri(link).LocalPath);
                    var relativePath = $"/pdfs/{filename}";
                    if (!foundPdfs.Any(p => p.FilePath == relativePath))
                    {
                        var folderPath = Path.Combine("wwwroot", "pdfs");
                        Directory.CreateDirectory(folderPath);
                        var localPath = Path.Combine(folderPath, filename);

                        try
                        {
                            using var pdfClient = new HttpClient();
                            var bytes = await pdfClient.GetByteArrayAsync(link);
                            await System.IO.File.WriteAllBytesAsync(localPath, bytes);

                            var text = ExtractTextFromPdf(localPath);
                            var topWords = GetTop10Words(text);

                            var pdfDoc = new PdfModel
                            {
                                FileName = filename,
                                FilePath = relativePath,
                                WordStats = topWords.Select(w => new WordStat
                                {
                                    Word = w.Key,
                                    Count = w.Value
                                }).ToList()
                            };

                            foundPdfs.Add(pdfDoc);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Fehler beim PDF-Download/Textanalyse: {ex.Message}");
                        }
                    }
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

        private string ExtractTextFromPdf(string path)
        {
            using var pdf = UglyToad.PdfPig.PdfDocument.Open(path);
            return string.Join(" ", pdf.GetPages().Select(p => p.Text));
        }

        private Dictionary<string, int> GetTop10Words(string text)
        {
            return text
                .ToLower()
                .Split(new[] { ' ', '\n', '\r', ',', '.', '!', '?', ';', ':', '(', ')', '[', ']', '{', '}', '"', '\'' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3)
                .GroupBy(w => w)
                .Select(g => new { Word = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(10)
                .ToDictionary(g => g.Word, g => g.Count);
        }
    }
}
