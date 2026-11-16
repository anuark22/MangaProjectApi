using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<bool> SendPasswordRecoveryEmail(string toEmail, string username, string token)
        {
            string frontendResetUrl = _config["EmailSettings:FrontendResetUrl"] ?? "http://localhost:5173/reset-password";
            string resetLink = $"{frontendResetUrl}?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(toEmail)}";

            string subject = "Recuperación de Contraseña - OurMangaSite";

            string htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #0f172a; color: #f8fafc; margin: 0; padding: 20px; }}
        .container {{ max-width: 580px; margin: 0 auto; background-color: #1e293b; border-radius: 12px; overflow: hidden; box-shadow: 0 10px 25px rgba(0,0,0,0.4); }}
        .header {{ background: linear-gradient(135deg, #6366f1 0%, #ec4899 100%); padding: 30px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 26px; color: #ffffff; letter-spacing: 1px; }}
        .content {{ padding: 30px; color: #cbd5e1; line-height: 1.6; font-size: 15px; }}
        .greeting {{ font-size: 18px; font-weight: bold; color: #ffffff; margin-bottom: 15px; }}
        .token-box {{ background-color: #0f172a; border: 1px dashed #6366f1; border-radius: 8px; padding: 15px; text-align: center; margin: 25px 0; }}
        .token-code {{ font-family: monospace; font-size: 24px; font-weight: bold; color: #38bdf8; letter-spacing: 3px; }}
        .btn {{ display: inline-block; background: linear-gradient(135deg, #6366f1 0%, #ec4899 100%); color: #ffffff !important; padding: 14px 28px; text-decoration: none; border-radius: 8px; font-weight: bold; margin: 15px 0; }}
        .footer {{ padding: 20px 30px; text-align: center; font-size: 12px; color: #64748b; border-top: 1px solid #334155; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>OurMangaSite</h1>
        </div>
        <div class='content'>
            <div class='greeting'>¡Hola, {username}!</div>
            <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta en OurMangaSite.</p>
            <p>Puedes usar el siguiente botón para restablecerla directamente:</p>
            <div style='text-align: center;'>
                <a href='{resetLink}' class='btn'>Restablecer Contraseña</a>
            </div>
            <p>O si prefieres, copia el siguiente código de seguridad en el formulario:</p>
            <div class='token-box'>
                <span class='token-code'>{token}</span>
            </div>
            <p style='font-size: 13px; color: #94a3b8;'>
                Este enlace y código expirarán en <strong>30 minutos</strong>.<br/>
                Si no solicitaste este cambio, puedes ignorar este mensaje; tu cuenta sigue estando segura.
            </p>
        </div>
        <div class='footer'>
            &copy; {DateTime.UtcNow.Year} OurMangaSite. Todos los derechos reservados.
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(toEmail, username, subject, htmlBody);
        }

        public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            string smtpServer = _config["EmailSettings:SmtpServer"] ?? string.Empty;
            int port = int.TryParse(_config["EmailSettings:Port"], out int p) ? p : 587;
            string senderEmail = _config["EmailSettings:SenderEmail"] ?? "no-reply@ourmangasite.com";
            string senderName = _config["EmailSettings:SenderName"] ?? "OurMangaSite";
            string username = _config["EmailSettings:Username"] ?? string.Empty;
            string password = _config["EmailSettings:Password"] ?? string.Empty;
            bool enableSsl = bool.TryParse(_config["EmailSettings:EnableSsl"], out bool ssl) ? ssl : true;

            // Si las credenciales SMTP no están configuradas, operamos en modo simulación para desarrollo
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogInformation(
                    "[EMAIL SIMULATION] Correo para: {ToEmail} ({ToName}) | Asunto: {Subject}",
                    toEmail, toName, subject
                );
                Console.WriteLine($"\n==================== [EMAIL SIMULATION] ====================");
                Console.WriteLine($"Para: {toEmail} ({toName})");
                Console.WriteLine($"Asunto: {subject}");
                Console.WriteLine($"============================================================\n");
                return true;
            }

            try
            {
                using var client = new SmtpClient(smtpServer, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = enableSsl
                };

                using var mail = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                mail.To.Add(new MailAddress(toEmail, toName));

                await client.SendMailAsync(mail);

                _logger.LogInformation("Correo enviado exitosamente a {ToEmail}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando correo a {ToEmail}: {Message}", toEmail, ex.Message);
                return false;
            }
        }
    }
}
