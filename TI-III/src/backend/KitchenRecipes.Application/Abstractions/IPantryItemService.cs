using KitchenRecipes.Application.DTOs;

namespace KitchenRecipes.Application.Abstractions;

public interface IPantryItemService
{
    Task<IReadOnlyList<PantryItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PantryItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PantryItemDto> CreateAsync(UpsertPantryItemRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, UpsertPantryItemRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
