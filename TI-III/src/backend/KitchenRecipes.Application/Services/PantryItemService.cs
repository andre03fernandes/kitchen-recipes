using KitchenRecipes.Application.Abstractions;
using KitchenRecipes.Application.DTOs;
using KitchenRecipes.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace KitchenRecipes.Application.Services;

public sealed class PantryItemService : IPantryItemService
{
    private readonly IApplicationDbContext _context;

    public PantryItemService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PantryItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PantryItems
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(ToDtoProjection())
            .ToListAsync(cancellationToken);
    }

    public async Task<PantryItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.PantryItems
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(ToDtoProjection())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PantryItemDto> CreateAsync(UpsertPantryItemRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var pantryItem = new PantryItem
        {
            Name = request.Name.Trim(),
            ImageUrl = NormalizeImageUrl(request.ImageUrl),
            AvailableQuantity = request.AvailableQuantity,
            Unit = request.Unit.Trim(),
            ExpirationDate = request.ExpirationDate,
        };

        _context.PantryItems.Add(pantryItem);
        await _context.SaveChangesAsync(cancellationToken);

        return new PantryItemDto(
            pantryItem.Id,
            pantryItem.Name,
            pantryItem.ImageUrl,
            pantryItem.AvailableQuantity,
            pantryItem.Unit,
            pantryItem.ExpirationDate,
            pantryItem.CreatedAtUtc,
            pantryItem.UpdatedAtUtc);
    }

    public async Task<bool> UpdateAsync(int id, UpsertPantryItemRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var pantryItem = await _context.PantryItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (pantryItem is null)
        {
            return false;
        }

        pantryItem.Name = request.Name.Trim();
        pantryItem.ImageUrl = NormalizeImageUrl(request.ImageUrl);
        pantryItem.AvailableQuantity = request.AvailableQuantity;
        pantryItem.Unit = request.Unit.Trim();
        pantryItem.ExpirationDate = request.ExpirationDate;
        pantryItem.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var pantryItem = await _context.PantryItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (pantryItem is null)
        {
            return false;
        }

        _context.PantryItems.Remove(pantryItem);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void Validate(UpsertPantryItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Pantry item name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Unit))
        {
            throw new ArgumentException("Pantry item unit is required.");
        }

        if (request.AvailableQuantity < 0)
        {
            throw new ArgumentException("Available quantity cannot be negative.");
        }

        if (!string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            if (!request.ImageUrl.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Pantry image must be a valid image data URL.");
            }

            if (request.ImageUrl.Length > 3_000_000)
            {
                throw new ArgumentException("Pantry image is too large.");
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

    private static Expression<Func<PantryItem, PantryItemDto>> ToDtoProjection()
    {
        return pantryItem => new PantryItemDto(
            pantryItem.Id,
            pantryItem.Name,
            pantryItem.ImageUrl,
            pantryItem.AvailableQuantity,
            pantryItem.Unit,
            pantryItem.ExpirationDate,
            pantryItem.CreatedAtUtc,
            pantryItem.UpdatedAtUtc);
    }
}
