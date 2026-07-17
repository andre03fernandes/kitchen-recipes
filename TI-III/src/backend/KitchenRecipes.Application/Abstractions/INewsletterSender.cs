namespace KitchenRecipes.Application.Abstractions;

public interface INewsletterSender
{
    Task SendAsync(string recipientEmail, string subject, string htmlBody, string plainTextBody, CancellationToken cancellationToken = default);
}
