using KitchenRecipes.Application.DTOs;

namespace KitchenRecipes.Application.Abstractions;

public interface IRecipeService
{
    Task<IReadOnlyList<RecipeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RecipeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<RecipeDto> CreateAsync(UpsertRecipeRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, UpsertRecipeRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
