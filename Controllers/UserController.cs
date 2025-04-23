using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCrawler.Data;
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

    [HttpGet("profile/{id}")]
    public async Task<IActionResult> GetProfile(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        return Ok(new { user.Id, user.Email, user.Nickname });
    }

    [HttpPut("profile/{id}")]
    public async Task<IActionResult> UpdateProfile(string id, [FromBody] ApplicationUser updated)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        if (!IsValidEmail(updated.Email))
            return BadRequest("Invalid email format.");

        // Email-Check (nicht bei sich selbst)
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

    [HttpGet("profile/{id}/history")]
    public async Task<IActionResult> GetSearchHistory(string id)
    {
        var history = await _context.SearchHistories
            .Where(h => h.UserId == id)
            .Include(h => h.Pdfs)
            .OrderByDescending(h => h.Date)
            .ToListAsync();

        var result = history.Select(h => new
        {
            h.Url,
            h.Date,
            Pdfs = h.Pdfs.Select(p => new { p.FileName, p.FilePath })
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
