using KitchenRecipes.Domain.Common;

namespace KitchenRecipes.Domain.Entities;

public sealed class AssistantChatThread : BaseEntity
{
    public int AppUserId { get; set; }
    public AppUser AppUser { get; set; } = default!;
    public string Title { get; set; } = string.Empty;
    public string LastMessagePreview { get; set; } = string.Empty;
    public ICollection<AssistantChatMessage> Messages { get; set; } = new List<AssistantChatMessage>();
}
