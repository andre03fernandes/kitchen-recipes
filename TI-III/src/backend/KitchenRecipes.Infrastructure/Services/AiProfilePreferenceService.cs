using KitchenRecipes.Application.Abstractions;
using KitchenRecipes.Application.DTOs;
using KitchenRecipes.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace KitchenRecipes.Infrastructure.Services;

public sealed class AiProfilePreferenceService : IAiProfilePreferenceService
{
    private readonly AiOptions _options;
    private string _activeProfile;

    public AiProfilePreferenceService(IOptions<AiOptions> options)
    {
        _options = options.Value;
        _activeProfile = NormalizeProfile(_options.ActiveProfile);
    }

    public AiProfileSettingsDto GetCurrent()
    {
        return BuildSettings();
    }

    public AiProfileSettingsDto UpdateProfile(string profile)
    {
        _activeProfile = NormalizeProfile(profile);
        return BuildSettings();
    }

    private AiProfileSettingsDto BuildSettings()
    {
        var activeModel = ResolveModel(_activeProfile);
        return new AiProfileSettingsDto(
            Enabled: _options.Enabled,
            Provider: _options.Provider,
            ActiveProfile: _activeProfile,
            FastModel: _options.FastModel,
            QualityModel: _options.QualityModel,
            ActiveModel: activeModel);
    }

    private string ResolveModel(string profile)
    {
        if (profile == "quality" && !string.IsNullOrWhiteSpace(_options.QualityModel))
        {
            return _options.QualityModel;
        }

        if (profile == "fast" && !string.IsNullOrWhiteSpace(_options.FastModel))
        {
            return _options.FastModel;
        }

        return _options.ResolveModel();
    }

    private static string NormalizeProfile(string profile)
    {
        if (string.Equals(profile, "quality", StringComparison.OrdinalIgnoreCase))
        {
            return "quality";
        }

        return "fast";
    }
}
