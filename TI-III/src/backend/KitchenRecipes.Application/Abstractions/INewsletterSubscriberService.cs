using KitchenRecipes.Application.DTOs;

namespace KitchenRecipes.Application.Abstractions;

public interface INewsletterSubscriberService
{
    Task<IReadOnlyList<NewsletterSubscriberDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<NewsletterSubscriberDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<NewsletterSubscriberDto> CreateAsync(UpsertNewsletterSubscriberRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, UpsertNewsletterSubscriberRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
