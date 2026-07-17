using KitchenRecipes.Application.Abstractions;
using KitchenRecipes.Application.DTOs;
using KitchenRecipes.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace KitchenRecipes.Application.Services;

public sealed class RecipeService : IRecipeService
{
    private readonly IApplicationDbContext _context;

    public RecipeService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RecipeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Recipes
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(ToDtoProjection())
            .ToListAsync(cancellationToken);
    }

    public async Task<RecipeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Recipes
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(ToDtoProjection())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<RecipeDto> CreateAsync(UpsertRecipeRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var recipe = new Recipe
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            ImageUrl = NormalizeImageUrl(request.ImageUrl),
            PreparationMinutes = request.PreparationMinutes,
            Servings = request.Servings,
        };

        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync(cancellationToken);

        return new RecipeDto(
            recipe.Id,
            recipe.Name,
            recipe.Description,
            recipe.ImageUrl,
            recipe.PreparationMinutes,
            recipe.Servings,
            recipe.CreatedAtUtc,
            recipe.UpdatedAtUtc);
    }

    public async Task<bool> UpdateAsync(int id, UpsertRecipeRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var recipe = await _context.Recipes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (recipe is null)
        {
            return false;
        }

        recipe.Name = request.Name.Trim();
        recipe.Description = request.Description.Trim();
        recipe.ImageUrl = NormalizeImageUrl(request.ImageUrl);
        recipe.PreparationMinutes = request.PreparationMinutes;
        recipe.Servings = request.Servings;
        recipe.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var recipe = await _context.Recipes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (recipe is null)
        {
            return false;
        }

        _context.Recipes.Remove(recipe);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void Validate(UpsertRecipeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Recipe name is required.");
        }

        if (request.PreparationMinutes <= 0)
        {
            throw new ArgumentException("Preparation minutes must be greater than 0.");
        }

        if (request.Servings <= 0)
        {
            throw new ArgumentException("Servings must be greater than 0.");
        }

        if (!string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            if (!request.ImageUrl.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Recipe image must be a valid image data URL.");
            }

            if (request.ImageUrl.Length > 3_000_000)
            {
                throw new ArgumentException("Recipe image is too large.");
            }
        }
    }

    private static string? NormalizeImageUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return null;
        }

        return imageUrl.Trim();
    }

    private static Expression<Func<Recipe, RecipeDto>> ToDtoProjection()
    {
        return recipe => new RecipeDto(
            recipe.Id,
            recipe.Name,
            recipe.Description,
            recipe.ImageUrl,
            recipe.PreparationMinutes,
            recipe.Servings,
            recipe.CreatedAtUtc,
            recipe.UpdatedAtUtc);
    }
}
