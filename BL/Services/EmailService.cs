using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BL.Services
{
    public interface IEmailService
    {
        Task SendFeedbackResponseAsync(string toEmail, string toName, string adminMessage, int rating);
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

        public async Task SendFeedbackResponseAsync(
            string toEmail, string toName, string adminMessage, int rating)
        {
            var smtpHost   = _config["Email:SmtpHost"]   ?? "smtp.gmail.com";
            var smtpPort   = int.Parse(_config["Email:SmtpPort"] ?? "587");
            var smtpUser   = _config["Email:Username"]   ?? "";
            var smtpPass   = _config["Email:Password"]   ?? "";
            var fromEmail  = _config["Email:From"]       ?? smtpUser;
            var fromName   = _config["Email:FromName"]   ?? "YK Almoayyed Showroom";

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials    = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl      = true,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            var stars = string.Concat(Enumerable.Repeat("⭐", rating));

            var body = $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'>
  <style>
    body {{ font-family: Arial, sans-serif; background:#f4f4f4; margin:0; padding:0; }}
    .container {{ max-width:600px; margin:30px auto; background:#fff; border-radius:8px;
                  padding:30px; box-shadow:0 2px 8px rgba(0,0,0,.1); }}
    .header {{ background:#1a3c6e; color:#fff; padding:20px; border-radius:6px 6px 0 0;
               text-align:center; margin:-30px -30px 24px; }}
    .header h1 {{ margin:0; font-size:20px; }}
    .rating {{ font-size:24px; margin:12px 0; }}
    .message-box {{ background:#f0f4ff; border-left:4px solid #1a3c6e;
                    padding:14px 18px; border-radius:4px; margin:16px 0; }}
    .footer {{ font-size:12px; color:#888; margin-top:24px; text-align:center; }}
  </style>
</head>
<body>
  <div class='container'>
    <div class='header'>
      <h1>YK Almoayyed &amp; Sons — Showroom</h1>
    </div>
    <p>Dear <strong>{WebUtility.HtmlEncode(toName)}</strong>,</p>
    <p>Thank you for visiting our showroom and taking the time to share your feedback.</p>
    <p>Your rating: <span class='rating'>{stars}</span> ({rating}/5)</p>
    <p>Our team has reviewed your comments and would like to respond:</p>
    <div class='message-box'>
      {WebUtility.HtmlEncode(adminMessage).Replace("\n", "<br>")}
    </div>
    <p>We truly value your experience and hope to see you again soon.</p>
    <p>Warm regards,<br><strong>YK Almoayyed &amp; Sons Team</strong></p>
    <div class='footer'>
      This email was sent in response to feedback you submitted at our showroom.
    </div>
  </div>
</body>
</html>";

            var mail = new MailMessage
            {
                From       = new MailAddress(fromEmail, fromName),
                Subject    = "Response to Your Showroom Feedback — YK Almoayyed & Sons",
                Body       = body,
                IsBodyHtml = true
            };
            mail.To.Add(new MailAddress(toEmail, toName));

            try
            {
                await client.SendMailAsync(mail);
                _logger.LogInformation("Feedback response email sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send feedback response email to {Email}", toEmail);
                throw;
            }
        }
    }
}
