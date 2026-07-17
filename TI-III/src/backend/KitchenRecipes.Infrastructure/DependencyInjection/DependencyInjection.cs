using KitchenRecipes.Application.Abstractions;
using KitchenRecipes.Infrastructure.Configuration;
using KitchenRecipes.Infrastructure.Data;
using KitchenRecipes.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KitchenRecipes.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing connection string: DefaultConnection");

        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<INewsletterSender, SmtpNewsletterSender>();
        services.AddScoped<INewsletterReportingService, NewsletterReportingService>();
        services.AddSingleton<IAiProfilePreferenceService, AiProfilePreferenceService>();
        services.AddHttpClient<OllamaAssistantTextGenerator>((provider, client) =>
        {
            var aiOptions = configuration.GetSection(AiOptions.SectionName).Get<AiOptions>() ?? new AiOptions();
            client.BaseAddress = new Uri(aiOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(180);
        });
        services.AddScoped<IAssistantTextGenerator>(provider => provider.GetRequiredService<OllamaAssistantTextGenerator>());

        return services;
    }
}
