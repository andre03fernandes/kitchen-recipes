using KitchenRecipes.Domain.Common;

namespace KitchenRecipes.Domain.Entities;

public sealed class AssistantChatMessage : BaseEntity
{
    public int AssistantChatThreadId { get; set; }
    public AssistantChatThread AssistantChatThread { get; set; } = default!;
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? PantryHighlightsJson { get; set; }
    public string? SuggestedRecipesJson { get; set; }
    public string? FollowUpPromptsJson { get; set; }
}
