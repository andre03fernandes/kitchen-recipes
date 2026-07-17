using KitchenRecipes.Application.Abstractions;
using KitchenRecipes.Application.Services.Newsletter;
using KitchenRecipes.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KitchenRecipes.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<SystemStatusService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IRecipeService, RecipeService>();
        services.AddScoped<IPantryItemService, PantryItemService>();
        services.AddScoped<IPantryAssistantService, PantryAssistantService>();
        services.AddScoped<IAssistantConversationService, AssistantConversationService>();
        services.AddScoped<INewsletterSubscriberService, NewsletterSubscriberService>();
        services.AddScoped<INewsletterCampaignService, NewsletterCampaignService>();
        return services;
    }
}
