using Backend_Mboagreen.Data;
using Backend_Mboagreen.Models;
using Backend_Mboagreen.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Backend_Mboagreen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConnexionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<UserModel> _hasher;
        private readonly IConfiguration _config;
        private readonly ILogger<ConnexionController> _logger;

        public ConnexionController(IConfiguration config, AppDbContext context, ILogger<ConnexionController> logger)
        {
            _context = context;
            _hasher = new PasswordHasher<UserModel>();
            _config = config;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ConnexionModel login)
        {
            if (login == null || !ModelState.IsValid)
                return BadRequest(new { message = "Données invalides" });

            if (string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Password))
                return BadRequest(new { message = "Email et mot de passe requis" });

            try
            {
                var user = await _context.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == login.Email);

                if (user == null)
                {
                    _logger.LogWarning($"Login attempt with non-existent email: {login.Email}");
                    // Message générique pour ne pas révéler l'existence d'utilisateurs
                    return BadRequest(new { message = "Identifiants incorrects" });
                }

                var result = _hasher.VerifyHashedPassword(user, user.Password, login.Password);

                if (result == PasswordVerificationResult.Failed)
                {
                    _logger.LogWarning($"Failed login attempt for user: {login.Email}");
                    return BadRequest(new { message = "Identifiants incorrects" });
                }

                var jwtKey = _config["Jwt:Key"];
                if (string.IsNullOrEmpty(jwtKey))
                    throw new Exception("JWT Key manquant");

                var key = Encoding.UTF8.GetBytes(jwtKey);
                var expirationMinutes = int.Parse(_config["Jwt:ExpirationMinutes"] ?? "1440");

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.UserType),
                        new Claim("UserId", user.Id.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                    Issuer = _config["Jwt:Issuer"] ?? "MboaGreen",
                    Audience = _config["Jwt:Audience"] ?? "MboaGreenUsers",
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature
                    )
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation($"Successful login for user: {login.Email}");

                return Ok(new
                {
                    message = "Connexion réussie",
                    token = tokenString,
                    expiresIn = expirationMinutes * 60, // en secondes
                    user = new
                    {
                        user.Id,
                        user.Nom,
                        user.Prenom,
                        user.Email,
                        user.UserType
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during login: {ex.Message}");
                return StatusCode(500, new { message = "Erreur lors de la connexion" });
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { message = "Token invalide" });

                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new { message = "Token invalide" });

                var user = await _context.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return NotFound(new { message = "Utilisateur introuvable" });

                return Ok(new
                {
                    user.Id,
                    user.Nom,
                    user.Prenom,
                    user.Email,
                    user.UserType,
                    user.Telephone,
                    user.Adresse,
                    user.Ville,
                    user.Quartier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetMe: {ex.Message}");
                return StatusCode(500, new { message = "Erreur lors de la récupération du profil" });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Le logout côté client supprime le token
            // Optionnel: implémenter un token blacklist si besoin
            return Ok(new { message = "Déconnexion réussie" });
        }
    }
}