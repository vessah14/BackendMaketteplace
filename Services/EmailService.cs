using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;

namespace Backend_Mboagreen.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string email, string resetLink, string userName);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetLink, string userName)
        {
            try
            {
                var smtpServer = _config["Email:SmtpServer"];
                var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "587");
                var smtpUsername = _config["Email:SmtpUsername"];
                var smtpPassword = _config["Email:SmtpPassword"];
                var senderEmail = _config["Email:SenderEmail"];
                var senderName = _config["Email:SenderName"];

                if (string.IsNullOrEmpty(smtpServer))
                {
                    _logger.LogWarning("Email service not configured properly");
                    return;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(new MailboxAddress(userName, email));
                message.Subject = "Réinitialiser votre mot de passe - Mboa Green";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <html>
                            <body style='font-family: Arial, sans-serif;'>
                                <h2>Réinitialisation de mot de passe</h2>
                                <p>Bonjour {userName},</p>
                                <p>Vous avez demandé la réinitialisation de votre mot de passe.</p>
                                <p>Cliquez sur le lien ci-dessous pour réinitialiser votre mot de passe (valide 30 minutes):</p>
                                <a href='{resetLink}' style='display: inline-block; padding: 10px 20px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px;'>
                                    Réinitialiser le mot de passe
                                </a>
                                <p>Si vous n'avez pas demandé cette réinitialisation, ignorez cet email.</p>
                                <hr>
                                <p><small>Cet email est automatique, ne pas répondre.</small></p>
                            </body>
                        </html>"
                };

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(smtpUsername, smtpPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                _logger.LogInformation($"Password reset email sent to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email to {email}: {ex.Message}");
                throw;
            }
        }
    }
}
