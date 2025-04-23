using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebCrawler.DTOs;
using WebCrawler.DTOs.WebCrawler.DTOs;
using WebCrawler.Models;

namespace WebCrawler.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;

    public AuthController(UserManager<ApplicationUser> userManager, IConfiguration config)
    {
        _userManager = userManager;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!IsValidEmail(dto.Email))
            return BadRequest("Invalid email format");

        var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
        if (existingEmail != null)
            return Conflict("Email already in use");

        var existingNickname = await _userManager.Users.FirstOrDefaultAsync(u => u.Nickname == dto.Nickname);
        if (existingNickname != null)
            return Conflict("Nickname already in use");

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            Nickname = dto.Nickname
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok("User registered successfully");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Email == dto.EmailOrNickname || u.Nickname == dto.EmailOrNickname);

        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized("Invalid credentials.");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Nickname ?? user.Email!)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            token = tokenString,
            user.Id,
            user.Email,
            user.Nickname
        });
    }

    private static Dictionary<string, (string Email, DateTime Expiry)> _resetTokens = new();

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound("Email not registered.");

        var token = Guid.NewGuid().ToString();
        var expiry = DateTime.UtcNow.AddMinutes(1); // 60 Sekunden gültig

        _resetTokens[token] = (user.Email, expiry);

        // Simulierte Antwort – in echt würdest du Mail verschicken
        return Ok(new
        {
            Message = "Simulierter Reset-Link erstellt.",
            Link = $"http://localhost:8080/reset-password.html?token={token}",
            Expiry = expiry
        });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (!_resetTokens.ContainsKey(dto.Token))
            return BadRequest("Ungültiger oder abgelaufener Token.");

        var (email, expiry) = _resetTokens[dto.Token];
        if (DateTime.UtcNow > expiry)
            return BadRequest("Token ist abgelaufen.");

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound("User nicht gefunden.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        _resetTokens.Remove(dto.Token);

        return Ok("Passwort erfolgreich zurückgesetzt.");
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
