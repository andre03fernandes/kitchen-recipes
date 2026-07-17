using KitchenRecipes.Application.Abstractions;

namespace KitchenRecipes.Tests.Fakes;

internal sealed class FakeNewsletterSender : INewsletterSender
{
    public List<(string RecipientEmail, string Subject, string HtmlBody, string PlainTextBody)> SentMessages { get; } = [];

    public Task SendAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        string plainTextBody,
        CancellationToken cancellationToken = default)
    {
        SentMessages.Add((recipientEmail, subject, htmlBody, plainTextBody));
        return Task.CompletedTask;
    }
}
