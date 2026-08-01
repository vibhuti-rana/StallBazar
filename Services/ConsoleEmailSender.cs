namespace StallBazar.Services;

public class ConsoleEmailSender : IEmailSender
{
    private readonly ILogger<ConsoleEmailSender> _logger;
    private readonly IConfiguration _configuration;

    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        var host = _configuration["Smtp:Host"];
        var username = _configuration["Smtp:Username"];
        var password = _configuration["Smtp:Password"]?.Replace(" ", string.Empty);
        var from = _configuration["Smtp:From"] ?? username;
        var port = int.TryParse(_configuration["Smtp:Port"], out var configuredPort) ? configuredPort : 587;

        if (!string.IsNullOrWhiteSpace(host) &&
            !string.IsNullOrWhiteSpace(username) &&
            !string.IsNullOrWhiteSpace(password) &&
            !string.IsNullOrWhiteSpace(from))
        {
            using var message = new System.Net.Mail.MailMessage(from, toEmail, subject, htmlMessage)
            {
                IsBodyHtml = true
            };
            using var client = new System.Net.Mail.SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = new System.Net.NetworkCredential(username, password)
            };
            try
            {
                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent to {Email}. Subject: {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to send email to {Email}. Subject: {Subject}", toEmail, subject);
            }
            return;
        }

        _logger.LogInformation("Email queued to {Email}. Subject: {Subject}. Body: {Body}", toEmail, subject, htmlMessage);
    }
}
