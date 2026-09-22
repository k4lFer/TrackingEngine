using App.Interfaces.Ports.Emails.Models;

namespace App.Interfaces.Ports.Emails;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}
