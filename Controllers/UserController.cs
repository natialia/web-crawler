using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCrawler.Data;
using WebCrawler.Models;

namespace WebCrawler.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == dto.EmailOrNickname || u.Nickname == dto.EmailOrNickname);

            if (user == null || user.PasswordHash != dto.Password)
                return Unauthorized("Invalid credentials.");

            return Ok(new { id = user.Id, nickname = user.Nickname });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            if (!IsValidEmail(user.Email))
                return BadRequest("Invalid email format.");

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                return Conflict("Email already exists.");

            if (await _context.Users.AnyAsync(u => u.Nickname == user.Nickname))
                return Conflict("Nickname already exists.");

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
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

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound("Email not registered.");

            var token = Guid.NewGuid().ToString();
            var expiry = DateTime.UtcNow.AddMinutes(1); // 60 Sekunden gültig

            // Simulierte Antwort
            return Ok(new
            {
                Message = "Simulierter Reset-Link erstellt.",
                Link = $"http://localhost:8080/ui/reset-password.html?token={token}",
                Expiry = expiry
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.Nickname = updatedUser.Nickname;
            user.Email = updatedUser.Email;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("profile/{id}")]
        public async Task<IActionResult> GetProfile(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPut("profile/{id}")]
        public async Task<IActionResult> UpdateProfile(int id, User updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // Email-Format prüfen
            if (!IsValidEmail(updatedUser.Email))
                return BadRequest("Invalid email format.");

            // Email darf nicht doppelt vorkommen
            if (await _context.Users.AnyAsync(u => u.Email == updatedUser.Email && u.Id != id))
                return Conflict("Email already exists.");

            // Nickname darf auch nicht doppelt sein
            if (await _context.Users.AnyAsync(u => u.Nickname == updatedUser.Nickname && u.Id != id))
                return Conflict("Nickname already exists.");

            user.Nickname = updatedUser.Nickname;
            user.Email = updatedUser.Email;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}