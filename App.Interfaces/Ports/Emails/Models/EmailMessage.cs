namespace App.Interfaces.Ports.Emails.Models;

public record EmailMessage(string To, string Subject, string BodyHtml);
