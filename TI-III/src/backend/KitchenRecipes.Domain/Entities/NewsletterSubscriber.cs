using KitchenRecipes.Domain.Common;

namespace KitchenRecipes.Domain.Entities;

public sealed class NewsletterSubscriber : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public bool IsConfirmed { get; set; }
}
