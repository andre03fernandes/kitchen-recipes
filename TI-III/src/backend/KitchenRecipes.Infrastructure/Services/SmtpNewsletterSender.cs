using KitchenRecipes.Application.Abstractions;
using KitchenRecipes.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace KitchenRecipes.Infrastructure.Services;

public sealed class SmtpNewsletterSender : INewsletterSender
{
    private readonly SmtpOptions _smtpOptions;

    public SmtpNewsletterSender(IOptions<SmtpOptions> smtpOptions)
    {
        _smtpOptions = smtpOptions.Value;
    }

    public async Task SendAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        string plainTextBody,
        CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(_smtpOptions.SenderEmail, _smtpOptions.SenderName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };

        mailMessage.To.Add(new MailAddress(recipientEmail));
        mailMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(plainTextBody, null, "text/plain"));

        using var smtpClient = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port)
        {
            EnableSsl = _smtpOptions.EnableStartTls,
            Credentials = new NetworkCredential(_smtpOptions.Username, _smtpOptions.AppPassword),
        };

        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            await smtpClient.SendMailAsync(mailMessage, cancellationToken);
        }
        catch (SmtpException)
        {
            throw new InvalidOperationException("Unable to send newsletter campaign. Check SMTP credentials and Gmail App Password settings.");
        }
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_smtpOptions.SenderEmail) ||
            string.IsNullOrWhiteSpace(_smtpOptions.Username) ||
            string.IsNullOrWhiteSpace(_smtpOptions.AppPassword))
        {
            throw new InvalidOperationException("SMTP configuration is incomplete. Configure sender email, username, and app password.");
        }
    }
}
