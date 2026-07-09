using Backend_Mboagreen.Models;
using Backend_Mboagreen.Services;
using Backend_Mboagreen.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Backend_Mboagreen.DTOs;
using System.Security.Cryptography;
using System.Text;

namespace Backend_Mboagreen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordResetController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<PasswordResetController> _logger;
        private readonly IConfiguration _config;

        public PasswordResetController(
            AppDbContext context,
            IEmailService emailService,
            ILogger<PasswordResetController> logger,
            IConfiguration config)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _config = config;
        }

        /// <summary>
        /// Demander un lien de réinitialisation de mot de passe
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ResetPasswordRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Chercher l'utilisateur (ne pas révéler si email existe - sécurité)
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null)
                {
                    _logger.LogInformation($"Password reset requested for non-existent email: {dto.Email}");
                    // Retourner un message générique pour ne pas révéler l'existence de l'email
                    return Ok(new { message = "Si cet email est enregistré, un lien de réinitialisation vous sera envoyé" });
                }

                // Supprimer les tokens expirés
                var expiredTokens = await _context.PasswordResets
                    .Where(r => r.Email == dto.Email && r.Expiration < DateTime.UtcNow)
                    .ToListAsync();
                if (expiredTokens.Any())
                {
                    _context.PasswordResets.RemoveRange(expiredTokens);
                    await _context.SaveChangesAsync();
                }

                // Générer un token sécurisé (utiliser une source aléatoire cryptographique)
                var randomBytes = new byte[32];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomBytes);
                }
                var token = Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');

                // Créer l'enregistrement de réinitialisation
                var reset = new PasswordReset
                {
                    Email = dto.Email,
                    Token = token,
                    Expiration = DateTime.UtcNow.AddMinutes(30)
                };

                _context.PasswordResets.Add(reset);
                await _context.SaveChangesAsync();

                // Construire le lien (récupérer le frontend URL depuis la config)
                var frontendUrl = _config["App:FrontendUrl"] ?? "http://localhost:3000";
                var resetLink = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(dto.Email)}";

                // Envoyer l'email
                await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink, $"{user.Prenom} {user.Nom}");

                _logger.LogInformation($"Password reset token sent to {dto.Email}");

                return Ok(new { message = "Si cet email est enregistré, un lien de réinitialisation vous sera envoyé" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ForgotPassword: {ex.Message}");
                return StatusCode(500, new { message = "Erreur lors du traitement" });
            }
        }

        /// <summary>
        /// Valider le token et réinitialiser le mot de passe
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordSubmitDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Valider le token
                var reset = await _context.PasswordResets
                    .FirstOrDefaultAsync(r => r.Email == dto.Email && r.Token == dto.Token);

                if (reset == null)
                {
                    _logger.LogWarning($"Invalid reset token attempted for {dto.Email}");
                    return BadRequest(new { message = "Token invalide ou expiré" });
                }

                // Vérifier l'expiration
                if (reset.Expiration < DateTime.UtcNow)
                {
                    _context.PasswordResets.Remove(reset);
                    await _context.SaveChangesAsync();
                    _logger.LogWarning($"Expired reset token used for {dto.Email}");
                    return BadRequest(new { message = "Le lien a expiré. Veuillez demander une nouvelle réinitialisation" });
                }

                // Récupérer l'utilisateur
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (user == null)
                {
                    return BadRequest(new { message = "Utilisateur introuvable" });
                }

                // Hasher le nouveau mot de passe
                var hasher = new PasswordHasher<UserModel>();
                user.Password = hasher.HashPassword(user, dto.NewPassword);

                // Supprimer tous les tokens de ce user
                var allResets = await _context.PasswordResets
                    .Where(r => r.Email == dto.Email)
                    .ToListAsync();
                _context.PasswordResets.RemoveRange(allResets);

                // Sauvegarder les changements
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password reset successfully for {dto.Email}");

                return Ok(new { message = "Mot de passe réinitialisé avec succès" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ResetPassword: {ex.Message}");
                return StatusCode(500, new { message = "Erreur lors de la réinitialisation" });
            }
        }

        /// <summary>
        /// Vérifier si un token est valide (optionnel, pour le frontend)
        /// </summary>
        [HttpPost("verify-token")]
        public async Task<IActionResult> VerifyToken([FromBody] VerifyTokenDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var reset = await _context.PasswordResets
                    .FirstOrDefaultAsync(r => r.Email == dto.Email && r.Token == dto.Token);

                if (reset == null || reset.Expiration < DateTime.UtcNow)
                {
                    return BadRequest(new { valid = false, message = "Token invalide ou expiré" });
                }

                return Ok(new { valid = true, message = "Token valide" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in VerifyToken: {ex.Message}");
                return StatusCode(500, new { message = "Erreur lors de la vérification" });
            }
        }
    }

    public class VerifyTokenDto
    {
        public string? Email { get; set; }
        public string? Token { get; set; }
    }
}