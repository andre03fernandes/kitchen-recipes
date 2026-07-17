using KitchenRecipes.Domain.Common;

namespace KitchenRecipes.Domain.Entities;

public sealed class AppUser : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<AssistantChatThread> AssistantChatThreads { get; set; } = new List<AssistantChatThread>();
}
