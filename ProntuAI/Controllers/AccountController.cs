using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProntuAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _config;

        public AccountController(UserManager<IdentityUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var user = new IdentityUser { UserName = req.Email, Email = req.Email };
            var res = await _userManager.CreateAsync(user, req.Password);
            if (!res.Succeeded) return BadRequest(res.Errors.Select(e => e.Description));
            return Ok(new { user.Id, user.Email });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _userManager.FindByEmailAsync(req.Email);
            if (user == null) return Unauthorized();
            var ok = await _userManager.CheckPasswordAsync(user, req.Password);
            if (!ok) return Unauthorized();

            var token = GenerateToken(user);
            return Ok(new { token });
        }

        private string GenerateToken(IdentityUser user)
        {
            var key = _config["Jwt:Key"] ?? Environment.GetEnvironmentVariable("PRONTUAI_JWT_KEY");
            var issuer = _config["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("PRONTUAI_JWT_ISSUER");
            var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Id), new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty) };
            var sec = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key ?? throw new InvalidOperationException("JWT key not configured")));
            var creds = new SigningCredentials(sec, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(issuer, issuer, claims, expires: DateTime.UtcNow.AddHours(12), signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public record RegisterRequest(string Email, string Password);
    public record LoginRequest(string Email, string Password);
}
