using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendPasswordRecoveryEmail(string toEmail, string username, string token);
        Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string htmlBody);
    }
}
