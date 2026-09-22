using App.Interfaces.Ports.Emails;
using App.Interfaces.Ports.Emails.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace App.Infrastructure.Adapters.Emails;

public class MailKitEmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;

    public MailKitEmailSender(IOptions<SmtpSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_settings.SenderName, _settings.From));
        email.To.Add(MailboxAddress.Parse(message.To));
        email.Subject = message.Subject;
        email.Body = new TextPart(TextFormat.Html) { Text = message.BodyHtml };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, ct);
        await smtp.AuthenticateAsync(_settings.Username, _settings.Password, ct);
        await smtp.SendAsync(email, ct);
        await smtp.DisconnectAsync(true, ct);
    }
}
