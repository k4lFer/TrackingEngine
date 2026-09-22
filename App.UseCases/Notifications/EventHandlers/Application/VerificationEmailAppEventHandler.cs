using App.Domain.User.Events;
using App.Interfaces.Ports.Emails;
using App.Interfaces.Ports.Emails.Models;
using Cortex.Mediator.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace App.UseCases.Notifications.EventHandlers.Application;

public class VerificationEmailAppEventHandler : INotificationHandler<VerificationEmailAppEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly IConfiguration _configuration;
    private readonly ILogger<VerificationEmailAppEventHandler> _logger;

    public VerificationEmailAppEventHandler(
        IEmailSender emailSender,
        ITemplateRenderer templateRenderer,
        IConfiguration configuration,
        ILogger<VerificationEmailAppEventHandler> logger)
    {
        _emailSender = emailSender;
        _templateRenderer = templateRenderer;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Handle(VerificationEmailAppEvent notification, CancellationToken cancellationToken)
    {
        var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
        var verificationLink = $"{frontendBaseUrl}/verify-email?token={notification.LinkToken}";

        var htmlBody = await _templateRenderer.RenderAsync("WelcomeEmail.html", new
        {
            Username = notification.Username,
            VerificationLink = verificationLink,
            Code = notification.OtpCode
        });

        var message = new EmailMessage(
            To: notification.Email,
            Subject: "Verifica tu correo electrónico",
            BodyHtml: htmlBody
        );

        _logger.LogInformation("Sending verification email to {Email}", notification.Email);
        await _emailSender.SendAsync(message, cancellationToken);
    }
}