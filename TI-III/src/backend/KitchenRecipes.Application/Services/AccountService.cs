using KitchenRecipes.Application.Abstractions;
using KitchenRecipes.Application.DTOs;
using KitchenRecipes.Domain.Common;
using KitchenRecipes.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Security.Cryptography;

namespace KitchenRecipes.Application.Services;

public sealed class AccountService : IAccountService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100000;

    private readonly IApplicationDbContext _context;

    public AccountService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AuthUserDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRegisterRequest(request);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var duplicateEmail = await _context.AppUsers
            .AnyAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (duplicateEmail)
        {
            throw new ArgumentException("An account with this email already exists.");
        }

        var hasUsers = await _context.AppUsers.AnyAsync(cancellationToken);
        var role = hasUsers ? UserRoles.User : UserRoles.Admin;

        var user = new AppUser
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = HashPassword(request.Password),
            Role = role,
            IsActive = true,
        };

        _context.AppUsers.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(user);
    }

    public async Task<AuthUserDto?> ValidateCredentialsAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ValidateLoginRequest(request);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _context.AppUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail && x.IsActive, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var validPassword = VerifyPassword(request.Password, user.PasswordHash);
        if (!validPassword)
        {
            return null;
        }

        return ToDto(user);
    }

    public async Task<AuthUserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _context.AppUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return user is null ? null : ToDto(user);
    }

    public async Task<IReadOnlyList<AuthUserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AppUsers
            .AsNoTracking()
            .OrderBy(x => x.FullName)
            .Select(x => new AuthUserDto(x.Id, x.FullName, x.Email, x.Role, x.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UpdateRoleAsync(int userId, UpdateUserRoleRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRoleRequest(request);

        var user = await _context.AppUsers.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        user.Role = request.Role;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static AuthUserDto ToDto(AppUser user)
    {
        return new AuthUserDto(user.Id, user.FullName, user.Email, user.Role, user.IsActive);
    }

    private static void ValidateRegisterRequest(RegisterUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new ArgumentException("Full name is required.");
        }

        ValidateEmail(request.Email);

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            throw new ArgumentException("Password must have at least 6 characters.");
        }
    }

    private static void ValidateLoginRequest(LoginRequest request)
    {
        ValidateEmail(request.Email);

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is required.");
        }
    }

    private static void ValidateRoleRequest(UpdateUserRoleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Role) || !UserRoles.IsValid(request.Role))
        {
            throw new ArgumentException("Invalid role.");
        }
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.");
        }

        try
        {
            _ = new MailAddress(email.Trim());
        }
        catch (FormatException)
        {
            throw new ArgumentException("Email format is invalid.");
        }
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string passwordHash)
    {
        var parts = passwordHash.Split('.');
        if (parts.Length != 3)
        {
            return false;
        }

        var validIterations = int.TryParse(parts[0], out var iterations);
        if (!validIterations)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[1]);
            var expectedHash = Convert.FromBase64String(parts[2]);
            var computedHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);
            return CryptographicOperations.FixedTimeEquals(computedHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
