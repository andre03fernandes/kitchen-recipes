using KitchenRecipes.Application.DTOs;

namespace KitchenRecipes.Application.Abstractions;

public interface IAiProfilePreferenceService
{
    AiProfileSettingsDto GetCurrent();
    AiProfileSettingsDto UpdateProfile(string profile);
}
