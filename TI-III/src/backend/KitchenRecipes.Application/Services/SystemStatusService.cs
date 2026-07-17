using KitchenRecipes.Application.DTOs;

namespace KitchenRecipes.Application.Services;

public sealed class SystemStatusService
{
    public Task<SystemStatusDto> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var status = new SystemStatusDto(
            Service: "KitchenRecipes API",
            Status: "Healthy",
            TimestampUtc: DateTime.UtcNow);

        return Task.FromResult(status);
    }
}
