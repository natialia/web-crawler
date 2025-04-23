using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;
using WebCrawler.Data;
using WebCrawler.DTOs;
using WebCrawler.Models;

namespace WebCrawler.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;

    public UserController(UserManager<ApplicationUser> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        return Ok(new { user.Id, user.Email, user.Nickname });
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] ApplicationUser updated)
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        if (!IsValidEmail(updated.Email))
            return BadRequest("Invalid email format.");

        var emailOwner = await _userManager.FindByEmailAsync(updated.Email);
        if (emailOwner != null && emailOwner.Id != id)
            return Conflict("Email already in use.");

        user.Email = updated.Email;
        user.UserName = updated.Email;
        user.Nickname = updated.Nickname;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [Authorize]
    [HttpGet("profile/history")]
    public async Task<IActionResult> GetSearchHistory()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var history = await _context.SearchHistories
            .Where(h => h.UserId == id)
            .Include(h => h.Pdfs)
                .ThenInclude(p => p.WordStats)
            .OrderByDescending(h => h.Date)
            .ToListAsync();

        var result = history.Select(h => new
        {
            h.Url,
            h.Date,
            Pdfs = h.Pdfs.Select(p => new
            {
                p.FileName,
                p.FilePath,
                WordStats = p.WordStats.Select(w => new { w.Word, w.Count })
            })
        });

        return Ok(result);
    }

    [Authorize]
    [HttpGet("profile/pdfs")]
    public async Task<IActionResult> GetUserPdfs()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var pdfs = await _context.Pdfs
            .Where(p => p.SearchHistory.UserId == userId)
            .Select(p => new
            {
                p.Id,
                p.FileName,
                p.FilePath,
                Date = p.SearchHistory.Date
            })
            .ToListAsync();

        return Ok(pdfs);
    }

    [Authorize]
    [HttpPost("profile/wordcloud")]
    public async Task<IActionResult> GetWordcloud([FromBody] List<int> pdfIds)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var pdfs = await _context.Pdfs
            .Where(p => pdfIds.Contains(p.Id) && p.SearchHistory.UserId == userId)
            .Include(p => p.WordStats)
            .ToListAsync();

        var allStats = pdfs
            .SelectMany(p => p.WordStats)
            .GroupBy(ws => ws.Word.ToLower())
            .Where(g => g.Key.Length > 1 && Regex.IsMatch(g.Key, @"^[a-zA-ZäöüÄÖÜß]+$"))
                .Select(g => new
                {
                    Word = g.Key,
                    Count = g.Sum(x => x.Count)
                })
            .OrderByDescending(g => g.Count)
            .Take(50)
            .ToList();

        return Ok(allStats);
    }

    [Authorize]
    [HttpPost("profile/wordcloud/time")]
    public async Task<IActionResult> GetWordcloudByTimeRange([FromBody] TimeRangeDto range)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var pdfs = await _context.Pdfs
            .Where(p => p.SearchHistory.UserId == userId &&
                        p.SearchHistory.Date >= range.Start &&
                        p.SearchHistory.Date <= range.End)
            .Include(p => p.WordStats)
            .ToListAsync();

        var allStats = pdfs
            .SelectMany(p => p.WordStats)
            .GroupBy(ws => ws.Word.ToLower())
            .Select(g => new
            {
                Word = g.Key,
                Count = g.Sum(x => x.Count)
            })
            .OrderByDescending(g => g.Count)
            .Take(50)
            .ToList();

        return Ok(allStats);
    }

    [Authorize]
    [HttpGet("search-topword")]
    public async Task<IActionResult> SearchTopWord([FromQuery] string word)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var lowerWord = word.ToLower();

        var matchingPdfs = await _context.SearchHistories
            .Where(h => h.UserId == userId)
            .SelectMany(h => h.Pdfs)
            .Include(p => p.WordStats)
            .Where(p => p.WordStats.Any(ws => ws.Word.ToLower() == lowerWord))
            .ToListAsync();

        var result = matchingPdfs.Select(p => new
        {
            p.FileName,
            p.FilePath,
            Count = p.WordStats.First(ws => ws.Word.ToLower() == lowerWord).Count
        });

        return Ok(result);
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
