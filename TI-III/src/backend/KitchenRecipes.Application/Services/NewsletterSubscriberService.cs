using KitchenRecipes.Application.Abstractions;
using KitchenRecipes.Application.DTOs;
using KitchenRecipes.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Net.Mail;

namespace KitchenRecipes.Application.Services;

public sealed class NewsletterSubscriberService : INewsletterSubscriberService
{
    private readonly IApplicationDbContext _context;

    public NewsletterSubscriberService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<NewsletterSubscriberDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.NewsletterSubscribers
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(ToDtoProjection())
            .ToListAsync(cancellationToken);
    }

    public async Task<NewsletterSubscriberDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.NewsletterSubscribers
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(ToDtoProjection())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<NewsletterSubscriberDto> CreateAsync(UpsertNewsletterSubscriberRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var existing = await _context.NewsletterSubscribers
            .AnyAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (existing)
        {
            throw new ArgumentException("Subscriber email already exists.");
        }

        var subscriber = new NewsletterSubscriber
        {
            Email = normalizedEmail,
            IsConfirmed = request.IsConfirmed,
        };

        _context.NewsletterSubscribers.Add(subscriber);
        await _context.SaveChangesAsync(cancellationToken);

        return new NewsletterSubscriberDto(
            subscriber.Id,
            subscriber.Email,
            subscriber.IsConfirmed,
            subscriber.CreatedAtUtc,
            subscriber.UpdatedAtUtc);
    }

    public async Task<bool> UpdateAsync(int id, UpsertNewsletterSubscriberRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var subscriber = await _context.NewsletterSubscribers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (subscriber is null)
        {
            return false;
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var duplicateEmail = await _context.NewsletterSubscribers
            .AnyAsync(x => x.Id != id && x.Email == normalizedEmail, cancellationToken);

        if (duplicateEmail)
        {
            throw new ArgumentException("Subscriber email already exists.");
        }

        subscriber.Email = normalizedEmail;
        subscriber.IsConfirmed = request.IsConfirmed;
        subscriber.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var subscriber = await _context.NewsletterSubscribers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (subscriber is null)
        {
            return false;
        }

        _context.NewsletterSubscribers.Remove(subscriber);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void Validate(UpsertNewsletterSubscriberRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Subscriber email is required.");
        }

        try
        {
            _ = new MailAddress(request.Email.Trim());
        }
        catch (FormatException)
        {
            throw new ArgumentException("Subscriber email format is invalid.");
        }
    }

    private static Expression<Func<NewsletterSubscriber, NewsletterSubscriberDto>> ToDtoProjection()
    {
        return subscriber => new NewsletterSubscriberDto(
            subscriber.Id,
            subscriber.Email,
            subscriber.IsConfirmed,
            subscriber.CreatedAtUtc,
            subscriber.UpdatedAtUtc);
    }
}
