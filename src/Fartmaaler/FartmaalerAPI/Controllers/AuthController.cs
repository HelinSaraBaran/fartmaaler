using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FartmaalerAPI.Data;
using FartmaalerAPI.DTOs;

namespace FartmaalerAPI.Controllers
{   // fortæller at klassen er en api controller
    [ApiController]

    // sætter ruten til api/auth
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // Database context bruges til at hente brugere fra databasen 
        private readonly AppDbContext _context;

        // configuration bruges til at hente jwt indstillinger fra appsettings.json
        private readonly IConfiguration _configuration;

        // constructoren modtager database context og configuration via dependency injection 
        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // post endpoint til login 
        // modtager brugernavn og password fra frontend 
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {    // finder brugeren i databasen ud fra brugernavnet 
                var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);

                // hvis brugeren ikke findes, returneres 401 unauthorized 
                if (user == null)
                    return Unauthorized(new { message = "Forkert brugernavn eller password" });

                // tjekker om password matcher det hash, der ligger i vores database 
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                    return Unauthorized(new { message = "Forkert brugernavn eller password" });

                // opretter en jwt token til den godkendte bruger
                var token = GenerateJwtToken(user);

                // samler login svaret med token, brugernavn og rolle
                var response = new LoginResponse
                {
                    Token = token,
                    Username = user.Username,
                    Role = user.Role
                };

                //returner login svaret til frontend 
                return Ok(response);
            }
            catch (Exception ex)
            {
                // returnerer 500 hvis der sker en uventet fejl 
                return StatusCode(500, new { message = "Der opstod en fejl", error = ex.Message });
            }
        }

        // privat metode der opretter et jwt token til brugeren 
        private string GenerateJwtToken(Models.User user)
        {    // henter den hemmelige jwt key fra appsettings.json og laver den om til bytes
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            // opretter signing credentials, som bruges til at signere tokenet 
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // claims er informationer om brugeren, som gemmes inde i tokenet 
            var claims = new[]
            {   
                // gemmer brugerens  navn i tokenet 
                new Claim(ClaimTypes.Name, user.Username),

                // gememr brugeres rolle, så systemet kan bruge auth
                new Claim(ClaimTypes.Role, user.Role),

                // gemmer brugerens id som ekstra claim 
                new Claim("userId", user.Id.ToString())
            };

            //opretter selve jwt toknet med issuer, audicene, claims, udløbstid og signatur 
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);

            // skriver tokenet om til en string, som kan sendes tilbage til frontend 
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}